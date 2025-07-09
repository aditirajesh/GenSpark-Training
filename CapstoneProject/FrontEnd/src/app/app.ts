import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AuthLayoutComponent } from "./auth-layout/auth-layout";

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, AuthLayoutComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected title = 'FrontEnd';
}
