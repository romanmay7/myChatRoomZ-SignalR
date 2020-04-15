import { Component, OnInit } from '@angular/core';
import { LoginService } from '../../services/login.service';
import { ChannelService } from '../../services/channel.service';
import { Router } from '@angular/router';


@Component({
  selector: 'login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {

  constructor(private log_service: LoginService, private ch_service: ChannelService, private router: Router) { }

  errorMessage: string = "";
  public creds = {
    username: "",
    password: ""
  }

  ngOnInit() {
  }

  onLogin() {

    //Call the Login Service
    // alert(this.creds.username);
    this.log_service.login(this.creds)
      .subscribe(success => {
        if (success) {
          //Save Username in Items Service to track logged in user
          this.log_service.username = this.creds.username;
          this.ch_service.chatterName = this.creds.username;
          this.log_service.loggedIn = true;

          this.router.navigate(["/"])

        }

      }, err => this.errorMessage = "Failed to login")
  }
}
