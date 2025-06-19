import { Injectable } from "@angular/core";
import { BehaviorSubject, Observable } from "rxjs";
import { User } from "../models/UserModel";
import { HttpClient, HttpHeaders } from "@angular/common/http";

@Injectable({
    providedIn: 'root'
})

export class UserService {
    private apiUrl = 'https://dummyjson.com/users';
    private userSubject = new BehaviorSubject<User[]>([]);
    public users$ = this.userSubject.asObservable();

    constructor(private http: HttpClient) {
    }

    getAllUsers():Observable<any> {
        return this.http.get(`${this.apiUrl}`);
    }

    addUser(user: User): Observable<any> {
        const headers = new HttpHeaders({
            'Content-Type': 'application/json'
        });

        return this.http.post (`${this.apiUrl}/add`, user, {headers});
    }

    updateUser(users: User[]) {
        this.userSubject.next(users);
    }

    getCurrentUsers(): User[] {
        return this.userSubject.value;
    }
}

