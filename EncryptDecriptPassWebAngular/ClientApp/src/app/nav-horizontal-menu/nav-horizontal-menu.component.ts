
import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ErrorDescription } from '../common-scripts/ErrorDescriptionInterface';

@Component({
  selector: 'app-nav-horizontal-menu',
  templateUrl: './nav-horizontal-menu.component.html',
  styleUrls: ['./nav-horizontal-menu.component.css']
})
export class NavHorizontalMenu {
  showUser: boolean = false;

  usuarioNombre = '';
  password = '';
  httpClient: HttpClient;
  urlService: string;
  
  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this.httpClient = http;
    this.urlService = baseUrl;
  }

  public LoginUser() {
    var urlTest = this.urlService + 'api/DecryptEncrypt/LoginUser'

    this.httpClient.post<ErrorDescription>(urlTest, { UserName: this.usuarioNombre, Password: this.password }).subscribe(result => {

      if (!result.isError) {
        this.showUser = true;
      }
      else {
        alert(result.descripcion);
      }


    }, error => console.error(error));

  }
}
