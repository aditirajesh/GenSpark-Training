import { Component } from '@angular/core';
import { First } from "./first/first";
import { CustomerComponent } from './customer/customer';
import { Recipies } from './recipies/recipies';

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  styleUrl: './app.css',
  imports: [ Recipies]
})
export class App {
  protected title = 'myApp';
}