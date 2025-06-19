import { Component } from '@angular/core';
import { Store } from '@ngrx/store';
import { User } from '../models/User';
import { addUser } from '../ngrx/user.action';

@Component({
  selector: 'app-add-user',
  imports: [],
  templateUrl: './add-user.html',
  styleUrl: './add-user.css'
})
export class AddUser {
    constructor(private store:Store) {
     
     }
     handelAddUser(){
       const newUSer = new User(102,"Doe","Doe","user");
      this.store.dispatch(addUser({ user: newUSer }));
     }
}
