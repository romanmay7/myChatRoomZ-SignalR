import { Injectable,Output, EventEmitter } from '@angular/core';
import { HttpEventType, HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class UploadService {

  public upload_progress: number;
  public upload_message: string;
  public attachment_name: string="";
  @Output() public onUploadFinished = new EventEmitter();


  constructor(private http: HttpClient)
  {

  }

  public uploadFile = (files) => {
    if (files.length === 0) {
      return;
    }

    let fileToUpload = <File>files[0];
    const formData = new FormData();
    formData.append('file', fileToUpload, fileToUpload.name);

    this.http.post("/api/UploadImage", formData, { reportProgress: true, observe: 'events' })
      .subscribe(event => {
        if (event.type === HttpEventType.UploadProgress)
          this.upload_progress = Math.round(100 * event.loaded / event.total);
        else if (event.type === HttpEventType.Response) {
          this.upload_message = 'Attachment Success:' + fileToUpload.name;
          this.attachment_name = fileToUpload.name;
          this.onUploadFinished.emit(event.body);
        }
      });
  }

}
