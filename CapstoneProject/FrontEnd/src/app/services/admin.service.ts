// services/admin-user.service.ts
import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { User} from '../models/UserModel';
import { UserSearchModel } from '../models/UserSearchModel';
import { UserUpdateRequestDto } from '../models/UserDTOs';


@Injectable({
  providedIn: 'root'
})
export class AdminUserService {
  private apiUrl = 'http://localhost:5295/api/users';

  constructor(private http: HttpClient) {}

  /**
   * Get all users with pagination (Admin only)
   */
  getAllUsers(pageNumber: number = 1, pageSize: number = 10): Observable<User[]> {
    const params = new HttpParams()
      .set('pageNumber', pageNumber.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<User[]>(`${this.apiUrl}/all`, { params });
  }

  /**
   * Search users (Admin only)
   */
  searchUsers(searchModel: UserSearchModel): Observable<User[]> {
    return this.http.post<User[]>(`${this.apiUrl}/search`, searchModel);
  }

  /**
   * Get a specific user by username
   */
  getUser(username: string): Observable<User> {
    return this.http.get<User>(`${this.apiUrl}/get/${encodeURIComponent(username)}`);
  }

  /**
   * Update user details
   */
  updateUser(username: string, updateData: UserUpdateRequestDto): Observable<User> {
    return this.http.put<User>(`${this.apiUrl}/update/${encodeURIComponent(username)}`, updateData);
  }

  /**
   * Delete user (Admin only)
   */
  deleteUser(username: string): Observable<User> {
    return this.http.delete<User>(`${this.apiUrl}/delete/${encodeURIComponent(username)}`);
  }

  /**
   * Format last seen time
   */
  formatLastSeen(updatedAt: string): string {
    const lastSeen = new Date(updatedAt);
    const now = new Date();
    const diffInMs = now.getTime() - lastSeen.getTime();
    const diffInDays = Math.floor(diffInMs / (1000 * 60 * 60 * 24));
    const diffInHours = Math.floor(diffInMs / (1000 * 60 * 60));
    const diffInMinutes = Math.floor(diffInMs / (1000 * 60));

    if (diffInDays > 0) {
      return `${diffInDays} day${diffInDays > 1 ? 's' : ''} ago`;
    } else if (diffInHours > 0) {
      return `${diffInHours} hour${diffInHours > 1 ? 's' : ''} ago`;
    } else if (diffInMinutes > 0) {
      return `${diffInMinutes} minute${diffInMinutes > 1 ? 's' : ''} ago`;
    } else {
      return 'Just now';
    }
  }

  /**
   * Format date for display
   */
  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString('en-GB', {
      day: 'numeric',
      month: 'short',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }
}