import { Injectable } from "@angular/core";
import { environment } from "../../environments/environment.prod";
import { BehaviorSubject, catchError, Observable, tap, throwError } from "rxjs";
import { User } from "../models/UserModel";
import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { UserAddRequestDto, UserUpdateRequestDto } from "../models/UserDTOs";
import { UserSearchModel } from "../models/UserSearchModel";
import { ApiError } from "../models/ApiResponse";


@Injectable({
    providedIn: 'root'
})
export class UserService {
    private readonly apiUrl = `${environment.apiBaseUrl}/api/users`
    private currentUserSubject = new BehaviorSubject<User|null>(null);
    public currentUser$ = this.currentUserSubject.asObservable();

    constructor(private http:HttpClient) {

    }

    //createuser
    createUser(userDto: UserAddRequestDto) : Observable<User> {
        console.log('UserService: Creating user with username:', userDto.username);
        return this.http.post<User>(`${this.apiUrl}/register`,userDto)
        .pipe(
            tap((createdUser:User) => {
                console.log("User has been created successfully! username: ",createdUser.username)
            }),
            catchError(this.handleError.bind(this))
        )
    }

    //deleteuser 
    deleteUser(username:string, deletedBy?:string):Observable<User> {
        console.log('UserService: Deleting user:', username);
        const url = `${this.apiUrl}/delete/${username}`;

        let params = new HttpParams();
        if (deletedBy) {
            params = params.set('deletedBy',deletedBy);
        }

        return this.http.delete<User>(url)
        .pipe(
            tap((deletedUser: User) => {
                console.log('UserService: Successfully deleted user:', deletedUser.username);

                if (this.currentUserSubject.value?.username === username) {
                    this.currentUserSubject.next(null);
                }
            }),
            catchError(this.handleError.bind(this))
        );
    }

    //getuserbyusername
    getUserByUsername(username:string): Observable<User> {
        console.log('UserService: Getting user by username:', username);
        const url = `${this.apiUrl}/get/${username}`

        return this.http.get<User>(url)
        .pipe(
            tap((getUser: User) => {
                console.log('UserService: Successfully retrieved user:', getUser.username);
            }),
            catchError(this.handleError.bind(this))
        );
    }

    //getallusers
    getAllUsers():Observable<User[]> {
        console.log('UserService: Getting all users');
        return this.http.get<User[]>(`${this.apiUrl}/all`)
        .pipe(
            tap((users: User[]) => {
                console.log('UserService: Successfully retrieved', users.length, 'users')
            }),
            catchError(this.handleError.bind(this))
        );
    }

    //updateuser
    updateUserDetails(username:string,updatedto:UserUpdateRequestDto):Observable<User> {
        console.log('UserService: Updating user:', username);
        const url = `${this.apiUrl}/update/${username}`;

        return this.http.put<User>(url,updatedto)
        .pipe(
            tap((updatedUser:User) => {
                console.log('UserService: Successfully updated user:', updatedUser.username);

                if (this.currentUserSubject.value?.username === username) {
                    this.currentUserSubject.next(updatedUser);
                }
            }),
            catchError(this.handleError.bind(this))
        );
    }

    //searchuser 
    searchUsers(searchModel: UserSearchModel): Observable<User[]> {
        console.log('UserService: Searching users with criteria:', searchModel);
        
        // Your controller uses POST for search, not GET with query params
        // Matches your controller route: "search"
        const url = `${this.apiUrl}/search`;
        
        return this.http.post<User[]>(url, searchModel)
        .pipe(
            tap((users: User[]) => {
            console.log('UserService: Search returned', users.length, 'users');
            }),
            catchError(this.handleError.bind(this))
        );
    }

    setCurrentUser(user: User | null): void {
        this.currentUserSubject.next(user);
    }

    getCurrentUser(): User | null {
        return this.currentUserSubject.value;
    }

    private handleError(error: HttpErrorResponse): Observable<never> {
        console.error('UserService: HTTP Error occurred:', error);
        
        let apiError: ApiError;
        
        if (error.error instanceof ErrorEvent) {
        // Client-side error (network issues, etc.)
        apiError = {
            message: 'A network error occurred. Please check your connection.',
            status: 0,
            details: error.error.message
        };
        } else {
        // Server-side error (your C# API returned an error)
        apiError = {
            message: error.error?.message || 'An unknown server error occurred.',
            status: error.status,
            details: error.error
        };
        
        // Handle specific HTTP status codes
        switch (error.status) {
            case 400:
            apiError.message = 'Invalid request data provided.';
            break;
            case 401:
            apiError.message = 'You are not authorized to perform this action.';
            break;
            case 404:
            apiError.message = 'The requested user was not found.';
            break;
            case 409:
            apiError.message = 'This username is already taken.';
            break;
            case 500:
            apiError.message = 'A server error occurred. Please try again later.';
            break;
        }
        }
    
        return throwError(() => apiError);
    }
}