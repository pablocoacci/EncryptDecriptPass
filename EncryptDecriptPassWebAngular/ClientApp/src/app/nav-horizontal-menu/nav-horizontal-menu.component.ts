
import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-nav-horizontal-menu',
  templateUrl: './nav-horizontal-menu.component.html',
  styleUrls: ['./nav-horizontal-menu.component.css']
})
export class NavHorizontalMenu {
  //usuarioNombre: string;
  usuarioNombre = '';
  password = '';
  httpClient: HttpClient;
  urlService: string;

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    this.httpClient = http;
    this.urlService = baseUrl;
  }

  public LoginUser() {
    //alert('hola que tal');

    //alert(this.usuarioNombre + ' ' + this.password);

    var urlTest = this.urlService +'api/DecryptEncrypt/LoginUser'

    this.httpClient.post(urlTest, { UserName: this.usuarioNombre, Password: this.password }).subscribe(result => {
      alert(result);
    }, error => console.error(error));
    
  }
}
