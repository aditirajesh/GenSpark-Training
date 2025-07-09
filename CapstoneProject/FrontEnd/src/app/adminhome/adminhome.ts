// components/admin-home/admin-home.component.ts
import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Subject, takeUntil, debounceTime, distinctUntilChanged } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { AdminUserService } from '../services/admin.service';
import { UserService } from '../services/user.service';
import { CurrentUser } from '../models/CurrentUser';
import { User } from '../models/UserModel';
import { UserSearchModel } from '../models/UserSearchModel';
import { UserAddRequestDto } from '../models/UserDTOs';

@Component({
  selector: 'app-admin-home',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './adminhome.html',
  styleUrls: ['./adminhome.css']
})
export class AdminHomeComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  private searchSubject$ = new Subject<string>();

  // User and Authentication
  currentUser: CurrentUser | null = null;
   
  // Loading states
  isLoadingUsers: boolean = true;
  isDeletingUser: string | null = null;
  isSearching: boolean = false;
  isCreatingUser: boolean = false;
  
  // User management
  users: User[] = [];
  filteredUsers: User[] = [];
  searchQuery: string = '';
  
  // Modal state
  showCreateUserModal: boolean = false;
  newUser: UserAddRequestDto = {
    username: '',
    phone: '',
    password: '',
    role: ''
  };
  
  // Pagination
  currentPage: number = 1;
  pageSize: number = 10;
  totalUsers: number = 0;
  totalPages: number = 0;
  
  // Statistics
  totalActiveUsers: number = 0;
  totalAdmins: number = 0;
  totalRegularUsers: number = 0;
  recentlyJoinedUsers: number = 0;

  constructor(
    private authService: AuthService,
    private adminUserService: AdminUserService,
    private userService: UserService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.initializeComponent();
    this.setupSearch();
    this.loadUsers();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private initializeComponent(): void {
    // Subscribe to current user
    this.authService.currentUser$
      .pipe(takeUntil(this.destroy$))
      .subscribe(user => {
        this.currentUser = user;
        if (!user || user.role !== 'Admin') {
          // Redirect non-admin users
          this.router.navigate(['/home']);
        }
      });
  }

  private setupSearch(): void {
    this.searchSubject$
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        takeUntil(this.destroy$)
      )
      .subscribe(searchTerm => {
        this.performSearch(searchTerm);
      });
  }

  private async loadUsers(): Promise<void> {
    try {
      this.isLoadingUsers = true;
      console.log(`Loading users - Page: ${this.currentPage}, Size: ${this.pageSize}`);
      
      const users = await this.adminUserService.getAllUsers(this.currentPage, this.pageSize).toPromise();
      
      if (users) {
        this.users = users;
        this.filteredUsers = [...users];
        this.calculateStatistics();
        console.log(`Loaded ${users.length} users successfully`);
      }
      
    } catch (error) {
      console.error('Error loading users:', error);
      this.users = [];
      this.filteredUsers = [];
    } finally {
      this.isLoadingUsers = false;
    }
  }

  private async performSearch(searchTerm: string): Promise<void> {
    if (!searchTerm.trim()) {
      this.filteredUsers = [...this.users];
      this.isSearching = false;
      return;
    }

    try {
      this.isSearching = true;
      console.log('Searching for users with term:', searchTerm);
      
      const searchModel: UserSearchModel = {
        username: searchTerm.trim()
      };
      
      const searchResults = await this.adminUserService.searchUsers(searchModel).toPromise();
      
      if (searchResults) {
        this.filteredUsers = searchResults;
        console.log(`Search returned ${searchResults.length} results`);
      }
      
    } catch (error) {
      console.error('Error searching users:', error);
      this.filteredUsers = [];
    } finally {
      this.isSearching = false;
    }
  }

  private calculateStatistics(): void {
    this.totalActiveUsers = this.users.filter(u => !u.isDeleted).length;
    this.totalAdmins = this.users.filter(u => u.role === 'Admin' && !u.isDeleted).length;
    this.totalRegularUsers = this.users.filter(u => u.role === 'User' && !u.isDeleted).length;
    
    // Calculate recently joined (last 7 days)
    const sevenDaysAgo = new Date();
    sevenDaysAgo.setDate(sevenDaysAgo.getDate() - 7);
    
    this.recentlyJoinedUsers = this.users.filter(u => {
      const createdDate = new Date(u.createdAt);
      return createdDate >= sevenDaysAgo && !u.isDeleted;
    }).length;
    
    console.log('Statistics calculated:', {
      totalActive: this.totalActiveUsers,
      admins: this.totalAdmins,
      users: this.totalRegularUsers,
      recentlyJoined: this.recentlyJoinedUsers
    });
  }

  // Event handlers
  onSearchInputChange(event: Event): void {
    const target = event.target as HTMLInputElement;
    this.searchQuery = target.value;
    this.searchSubject$.next(this.searchQuery);
  }

  clearSearch(): void {
    this.searchQuery = '';
    this.filteredUsers = [...this.users];
    this.isSearching = false;
  }

  async deleteUser(username: string): Promise<void> {
    if (!confirm(`Are you sure you want to delete user "${username}"? This action cannot be undone.`)) {
      return;
    }

    if (this.currentUser?.username === username) {
      alert('You cannot delete your own account.');
      return;
    }

    try {
      this.isDeletingUser = username;
      console.log('Deleting user:', username);
      
      await this.adminUserService.deleteUser(username).toPromise();
      
      // Remove user from local arrays
      this.users = this.users.filter(u => u.username !== username);
      this.filteredUsers = this.filteredUsers.filter(u => u.username !== username);
      
      // Recalculate statistics
      this.calculateStatistics();
      
      console.log('User deleted successfully:', username);
      
      // Show success message
      alert(`User "${username}" has been deleted successfully.`);
      
    } catch (error) {
      console.error('Error deleting user:', error);
      alert('Failed to delete user. Please try again.');
    } finally {
      this.isDeletingUser = null;
    }
  }

  async changePage(newPage: number): Promise<void> {
    if (newPage < 1 || newPage === this.currentPage) {
      return;
    }
    
    this.currentPage = newPage;
    await this.loadUsers();
  }

  async refreshUsers(): Promise<void> {
    this.currentPage = 1;
    this.searchQuery = '';
    await this.loadUsers();
  }

  // Create User Modal Methods
  createUser(): void {
    console.log('Opening create user modal...');
    this.showCreateUserModal = true;
    this.resetNewUserForm();
  }

  closeCreateUserModal(): void {
    this.showCreateUserModal = false;
    this.resetNewUserForm();
  }

  private resetNewUserForm(): void {
    this.newUser = {
      username: '',
      phone: '',
      password: '',
      role: ''
    };
  }

  async submitCreateUser(): Promise<void> {
    if (!this.newUser.username || !this.newUser.phone || !this.newUser.password || !this.newUser.role) {
      alert('Please fill in all required fields.');
      return;
    }

    try {
      this.isCreatingUser = true;
      console.log('Creating new user:', this.newUser.username);

      const createdUser = await this.userService.createUser(this.newUser).toPromise();
      
      if (createdUser) {
        console.log('User created successfully:', createdUser);
        
        // Add the new user to the local arrays
        this.users.push(createdUser);
        this.filteredUsers = [...this.users];
        
        // Recalculate statistics
        this.calculateStatistics();
        
        // Close modal and show success message
        this.closeCreateUserModal();
        alert(`User "${createdUser.username}" has been created successfully!`);
        
        // Optionally refresh the users list to get the latest data from server
        // await this.loadUsers();
      }
      
    } catch (error: any) {
      console.error('Error creating user:', error);
      
      // Handle specific error messages from the API
      let errorMessage = 'Failed to create user. Please try again.';
      
      if (error?.message) {
        errorMessage = error.message;
      } else if (error?.status === 409) {
        errorMessage = 'This username is already taken. Please choose a different username.';
      } else if (error?.status === 400) {
        errorMessage = 'Invalid user data provided. Please check your input.';
      }
      
      alert(errorMessage);
    } finally {
      this.isCreatingUser = false;
    }
  }

  // Navigation methods
  navigateToUserExpenses(username: string): void {
    console.log('Admin navigating to expenses for user:', username);
    // Navigate to expenses page with admin flag and target username
    this.router.navigate(['/expenses'], { 
      queryParams: { 
        adminView: 'true',
        targetUser: username 
      } 
    });
  }

  navigateToUserReports(username: string): void {
    console.log('Admin navigating to reports for user:', username);
    // Navigate to reports page with admin flag and target username
    this.router.navigate(['/reports'], { 
      queryParams: { 
        adminView: 'true',
        targetUser: username 
      } 
    });
  }

  // Utility methods
  getUserRoleBadgeClass(role: string): string {
    switch (role?.toLowerCase()) {
      case 'admin':
        return 'role-badge admin';
      case 'user':
        return 'role-badge user';
      default:
        return 'role-badge default';
    }
  }

  formatLastSeen(updatedAt: string): string {
    return this.adminUserService.formatLastSeen(updatedAt);
  }

  formatJoinDate(createdAt: Date): string {
    const dateString = createdAt instanceof Date ? createdAt.toISOString() : String(createdAt);
    return this.adminUserService.formatDate(dateString);
  }

  isUserOnline(updatedAt: Date): boolean {
    // Convert to string if it's not already
    const dateString = updatedAt instanceof Date ? updatedAt.toISOString() : String(updatedAt);
    return this.adminUserService.formatLastSeen(dateString) === 'Just now';
  }

  formatPhone(phone: string): string {
    if (!phone) return 'Not provided';
    
    if (phone.length === 10) {
      return `+91 ${phone.slice(0, 5)} ${phone.slice(5)}`;
    }
    return phone;
  }

  trackByUsername(index: number, user: User): string {
    return user.username;
  }

  logout(): void {
    console.log('Admin logging out...');
    this.authService.logout();
  }

  // Computed properties
  get displayUsername(): string {
    if (!this.currentUser?.username) return 'Admin';
    
    const username = this.currentUser.username;
    const atIndex = username.indexOf('@');
    return atIndex > -1 ? username.substring(0, atIndex) : username;
  }

  get hasUsers(): boolean {
    return this.filteredUsers.length > 0;
  }

  get showNoResults(): boolean {
    return !this.isLoadingUsers && !this.hasUsers && this.searchQuery.trim().length > 0;
  }

  get showNoUsers(): boolean {
    return !this.isLoadingUsers && !this.hasUsers && this.searchQuery.trim().length === 0;
  }

  get paginationInfo(): string {
    if (this.totalUsers === 0) return 'No users';
    
    const start = (this.currentPage - 1) * this.pageSize + 1;
    const end = Math.min(this.currentPage * this.pageSize, this.totalUsers);
    
    return `Showing ${start}-${end} of ${this.totalUsers} users`;
  }
}