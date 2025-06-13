import { Component } from '@angular/core';

@Component({
  selector: 'app-customer',
  templateUrl: './customer.html',
  styleUrls: ['./customer.css']
})
export class CustomerComponent {
  likes = 0;
  dislikes = 0;

  like() {
    this.likes++;
  }

  dislike() {
    this.dislikes++;
  }
}
