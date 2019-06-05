
import { Component } from '@angular/core';

@Component({
  selector: 'app-nav-horizontal-menu',
  templateUrl: './nav-horizontal-menu.component.html',
  styleUrls: ['./nav-horizontal-menu.component.css']
})
export class NavHorizontalMenu {
  //usuarioNombre: string;
  usuarioNombre = '';
  password = '';

  public LoginUser() {
    alert('hola que tal');

    alert(this.usuarioNombre + ' ' + this.password);
  }
}
