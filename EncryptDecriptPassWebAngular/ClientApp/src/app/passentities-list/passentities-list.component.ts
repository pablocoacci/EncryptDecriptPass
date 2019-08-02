import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'passentities-list',
  templateUrl: './passentities-list.component.html'
})
export class PassEntitiesListComponent {
  public passList: PassEntities[];

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    http.get<PassEntities[]>(baseUrl + 'api/DecryptEncrypt/GetPasswordsEntities').subscribe(result => {
      this.passList = result;
    }, error => console.error(error));
  }
}

interface PassEntities {
  //id: number;
  //encryptDecryptPass: string;
  //usuario: string;
  //descripcion: string;
  //sitio: string;
  //cuenta: string;
  //passWord: string;
  //preguntaSecreta: string;
  //respuestaSecreta: string;
  //mailContacto: string;
  //isEncrypter: boolean;
}
