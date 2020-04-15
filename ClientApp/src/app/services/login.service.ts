import { HttpClient, HttpHeaders } from "@angular/common/http";
import { Injectable, OnInit } from '@angular/core';
import { Channel } from '../data-models/channel.model';
import { Observable } from "rxjs";
import { map } from "rxjs/operators";

@Injectable({
  providedIn: 'root'
})
export class LoginService
{
  private token: string = "";
  private tokenExpiration: Date;
  public username: string;
  public loggedIn: boolean;

  constructor(private http: HttpClient) {}

  public get loginRequired(): boolean {
    return this.token.length == 0 || this.tokenExpiration > new Date();
  }

  login(creds): Observable<boolean> {
    return this.http
      .post("/Account/CreateToken", creds)
      .pipe(
        map((data: any) => {
          this.token = data.token;
          this.tokenExpiration = data.expiration;
          //this.currentUser = creds.username;
          return true;
        })

      );
  }

  resetToken() {
    this.token = "";
  }


}
