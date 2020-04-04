import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormControl, Validators } from "@angular/forms";
import { ActivatedRoute } from '@angular/router';
import * as Signal_R from '@aspnet/signalr';
import { Message } from '../../data-models/message.model';
import { ChannelService } from '../../services/channel.service';
import { Channel } from '../../data-models/channel.model';

@Component({
  selector: 'app-channel',
  templateUrl: './channel.component.html',
  styleUrls: ['./channel.component.css']
})
export class ChannelComponent implements OnInit {

  public channel_id: string;
  public current_channel:Channel;

  public connection: any;//SignalR Hub Client's Connection Object
  public isConnected:boolean=false 
  public new_message: Message;
  public message_history: Message[] = [];
  public connected_clients: string[] = [];
  chatForm: FormGroup;

 

  constructor(private formBuilder: FormBuilder, private channelService: ChannelService, private activatedRoute: ActivatedRoute) { }

  async ngOnInit() {

    //Track changes of id parameter in Route
    this.activatedRoute.params.subscribe(async params => {
      this.channel_id = params['id'];
      //Request  Channel's data by ID from the Service
      if (this.channel_id) { this.current_channel = await this.channelService.find(this.channel_id) };
      this.message_history = this.current_channel.messageHistory//in case the Channel already have Message History
      //If Connection Object already exists, invoke JoinChannel function on the Hub,passing to it a Name of the Client and Channel ID
      if (this.isConnected) { console.log(this.connection + ":connected to hub already"); this.connection.invoke('JoinChannel', this.channelService.chatterName, this.channel_id); }
    });


    this.chatForm = this.formBuilder.group({ msgText: [] });//Build Form Object for Chat Messages

    // Initialize the SignalR Client
    //******************************************************************************************************
    //1.Creating Connection Object
    if (!this.isConnected) {
      this.connection = new Signal_R.HubConnectionBuilder()
        .withUrl('/chatHub')
        .build();
      //2.Initialize Connection
      this.connection
        .start()
        //When new Client Connects to the Hub we will invoke JoinChannel function on the Hub,passing to it a Name of the Client and Channel ID
        .then(() => { this.isConnected = true; this.connection.invoke('JoinChannel', this.channelService.chatterName, this.channel_id); })
        .catch(err => console.log('Error while starting connection: ' + err))


      //3.Listening to 'JoinChannel' Request from the Hub.
      //When it invoked, recieving the Names of Newly Connected Clients and adding them to 'connected_clients' array
      this.connection.on('JoinChannel', (newClient: string) => {
        console.log("Client:" + newClient);
        if (this.connected_clients.indexOf(newClient) === -1) this.connected_clients.push(newClient);
      });


      //4.On Recieving Message from Clients who calls 'SendMessage on the Hub'
      this.connection.on('RecieveMessage', (msg: Message) => {
        this.new_message = msg;
        this.message_history.push(msg);
      });
    }
  }
  //**************************************************************************************************************

  onSubmit() {
    this.sendMessage(this.chatForm.get('msgText').value);
  }


  sendMessage(chatform_message) {
    console.log(this.chatForm.get('msgText').value);
    // invoke 'SendMessage' on the Hub
    this.connection.invoke('SendMessage', this.channelService.chatterName, chatform_message, this.channel_id);

  
}

}

