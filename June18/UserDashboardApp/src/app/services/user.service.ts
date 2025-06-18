import { Injectable } from "@angular/core";
import { BehaviorSubject, Observable } from "rxjs";
import { User } from "../models/UserModel";
import { HttpClient, HttpHeaders } from "@angular/common/http";

@Injectable({
  providedIn: 'root'
})

export class UserService {
  private apiUrl = 'https://dummyjson.com/users';
  private usersSubject = new BehaviorSubject<User[]>([]);
  public users$ = this.usersSubject.asObservable();

  constructor(private http: HttpClient) {}

  // Get all users
  getAllUsers(): Observable<any> {
    return this.http.get(`${this.apiUrl}`);
  }

  // Add a new user
  addUser(userData: User): Observable<any> {
    const headers = new HttpHeaders({
      'Content-Type': 'application/json'
    });

    return this.http.post(`${this.apiUrl}/add`, userData, { headers });
  }

  // Update users list
  updateUsersList(users: User[]) {
    this.usersSubject.next(users);
  }

  // Get current users
  getCurrentUsers(): User[] {
    return this.usersSubject.value;
  }
}

