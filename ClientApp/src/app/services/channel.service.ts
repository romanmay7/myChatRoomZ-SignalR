import { HttpClient, HttpHeaders } from "@angular/common/http";
import { Injectable, OnInit } from '@angular/core';
import { Channel } from '../data-models/channel.model';

@Injectable({
  providedIn: 'root'
})
export class ChannelService   {

  public chatterName: string = 'Visitor';
  public channels: Channel[];

  constructor(private http: HttpClient) {
    this.chatterName += Math.floor(100 * Math.random());//Randomly Generated(Should be Current Logged In User)
  }



  async loadChannels() {
    this.channels = await this.http.get<Channel[]>("/api/channels").toPromise();
    console.log("Channels:"+this.channels);

  }

  async findAll(): Promise<Channel[]> {
    await this.loadChannels();
    return this.channels;
  }

  async find(id: string): Promise<Channel> {

    await this.loadChannels();
    let channel_id = await this.getSelectedIndex(id);

    return this.channels[channel_id];
  }

  async getSelectedIndex(id: string) {
    for (var i = 0; i < this.channels.length; i++) {
      if (this.channels[i].id == id) {

        return i;
      }
    }
    return -1;
  }


}
