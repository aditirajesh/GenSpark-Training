import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { Subject, takeUntil, tap } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { UserService } from '../services/user.service';
import { CurrentUser } from '../models/CurrentUser';
import { UserUpdateRequestDto } from '../models/UserDTOs';
import { User } from '../models/UserModel';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-user-profile',
  templateUrl: './userprofile.html',
  styleUrls: ['./userprofile.css'],
  imports: [CommonModule,FormsModule,ReactiveFormsModule]
})
export class UserProfileComponent implements OnInit, OnDestroy {
  currentUser: CurrentUser | null = null;
  userDetails: User | null = null;
  isLoading = false;
  error = '';
  successMessage = '';
  
  // Modal-specific errors
  currentPasswordError = '';
  newPasswordError = '';
  phoneError = '';
  
  // Form groups
  currentPasswordForm: FormGroup;
  newPasswordForm: FormGroup;
  phoneForm: FormGroup;
  
  // Modal states
  showCurrentPasswordModal = false;
  showNewPasswordModal = false;
  showPhoneModal = false;
  showDeleteConfirmation = false;
  
  // Password verification state
  verifiedCurrentPassword = '';

  private destroy$ = new Subject<void>();

  constructor(
    private authService: AuthService,
    private userService: UserService,
    private formBuilder: FormBuilder,
    private router: Router
  ) {
    this.currentPasswordForm = this.formBuilder.group({
      currentPassword: ['', [Validators.required, Validators.minLength(6)]]
    });

    this.newPasswordForm = this.formBuilder.group({
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]]
    }, { validators: this.passwordMatchValidator });

    this.phoneForm = this.formBuilder.group({
      phone: ['', [Validators.required, Validators.pattern(/^\+?[\d\s\-\(\)]+$/)]]
    });
  }

  ngOnInit(): void {
    this.loadUserProfile();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadUserProfile(): void {
    // Get current user from auth service (for username, role)
    this.authService.currentUser$
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (user) => {
          this.currentUser = user;
          console.log('CurrentUser loaded:', user);
          
          // If we have a user and a valid username, fetch complete details
          if (user?.username) {
            this.fetchUserDetails(user.username);
          } else if (user && !user.username) {
            // Handle case where JWT parsing failed
            console.error('JWT parsing failed - username is undefined');
            this.error = 'Failed to load user details from authentication token. Please try logging in again.';
          }
        },
        error: (error) => {
          console.error('Error loading user profile:', error);
          this.error = 'Failed to load user profile';
        }
      });
  }

  private fetchUserDetails(username: string): void {
    this.userService.getUserByUsername(username)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (userDetails) => {
          console.log('User details loaded:', userDetails);
          this.userDetails = userDetails;
          
          // Pre-populate phone form if phone exists
          if (userDetails.phone) {
            this.phoneForm.patchValue({ phone: userDetails.phone });
          }
        },
        error: (error) => {
          console.error('Error fetching user details:', error);
          this.error = 'Failed to load complete user information. Some features may not be available.';
        }
      });
  }

  // Check if current user is admin
  get isAdmin(): boolean {
    return this.currentUser?.role === 'Admin';
  }

  // Get display name for the user
  getUserDisplayName(): string {
    if (this.userDetails?.username) {
      return this.userDetails.username;
    }
    if (this.currentUser?.username) {
      return this.currentUser.username;
    }
    return 'Unknown User';
  }

  // Check if user data is properly loaded
  isUserDataComplete(): boolean {
    return !!(this.currentUser?.username && this.userDetails);
  }

  // Password validation
  private passwordMatchValidator(form: FormGroup) {
    const newPassword = form.get('newPassword');
    const confirmPassword = form.get('confirmPassword');
    
    if (newPassword && confirmPassword && newPassword.value !== confirmPassword.value) {
      confirmPassword.setErrors({ passwordMismatch: true });
      return { passwordMismatch: true };
    }
    
    return null;
  }

  // Open modals
  openPasswordModal(): void {
    this.showCurrentPasswordModal = true;
    this.currentPasswordForm.reset();
    this.verifiedCurrentPassword = '';
    this.clearAllMessages();
  }

  openNewPasswordModal(): void {
    this.showNewPasswordModal = true;
    this.newPasswordForm.reset();
    this.clearAllMessages();
  }

  openPhoneModal(): void {
    this.showPhoneModal = true;
    if (this.userDetails?.phone) {
      this.phoneForm.patchValue({ phone: this.userDetails.phone });
    }
    this.clearAllMessages();
  }

  openDeleteConfirmation(): void {
    this.showDeleteConfirmation = true;
    this.clearAllMessages();
  }

  // Close modals
  closeCurrentPasswordModal(): void {
    this.showCurrentPasswordModal = false;
    this.currentPasswordForm.reset();
    this.verifiedCurrentPassword = '';
    this.currentPasswordError = '';
  }

  closeNewPasswordModal(): void {
    this.showNewPasswordModal = false;
    this.newPasswordForm.reset();
    this.newPasswordError = '';
  }

  closePhoneModal(): void {
    this.showPhoneModal = false;
    this.phoneForm.reset();
    this.phoneError = '';
  }

  closeDeleteConfirmation(): void {
    this.showDeleteConfirmation = false;
  }

  // Verify current password
  verifyCurrentPassword(): void {
    if (this.currentPasswordForm.invalid || !this.currentUser?.username) {
      this.markFormGroupTouched(this.currentPasswordForm);
      return;
    }

    this.isLoading = true;
    this.currentPasswordError = '';

    // Use the login service to verify current password
    const loginCredentials = {
      username: this.currentUser.username,
      password: this.currentPasswordForm.value.currentPassword
    };

    this.authService.login(loginCredentials)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          // Current password is correct
          this.verifiedCurrentPassword = this.currentPasswordForm.value.currentPassword;
          this.closeCurrentPasswordModal();
          this.openNewPasswordModal();
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Current password verification failed:', error);
          this.currentPasswordError = 'Current password is incorrect. Please try again.';
          this.isLoading = false;
        }
      });
  }

  // Update password (after verification)
  updatePassword(): void {
    if (this.newPasswordForm.invalid || !this.currentUser?.username) {
      this.markFormGroupTouched(this.newPasswordForm);
      return;
    }

    this.isLoading = true;
    this.newPasswordError = '';

    const updateData: UserUpdateRequestDto = {
      password: this.newPasswordForm.value.newPassword
    };

    this.userService.updateUserDetails(this.currentUser.username, updateData)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.successMessage = 'Password updated successfully!';
          this.closeNewPasswordModal();
          this.isLoading = false;
          this.verifiedCurrentPassword = '';
          
          // Clear success message after 3 seconds
          setTimeout(() => this.clearAllMessages(), 3000);
        },
        error: (error) => {
          console.error('Error updating password:', error);
          this.newPasswordError = error.message || 'Failed to update password';
          this.isLoading = false;
        }
      });
  }

  // Update phone
  updatePhone(): void {
    if (this.phoneForm.invalid || !this.currentUser?.username) {
      this.markFormGroupTouched(this.phoneForm);
      return;
    }

    this.isLoading = true;
    this.phoneError = '';

    const updateData: UserUpdateRequestDto = {
      phone: this.phoneForm.value.phone
    };

    this.userService.updateUserDetails(this.currentUser.username, updateData)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (updatedUser) => {
          this.successMessage = 'Phone number updated successfully!';
          this.closePhoneModal();
          this.isLoading = false;
          
          // Update the user details
          if (this.userDetails) {
            this.userDetails = { ...this.userDetails, phone: updatedUser.phone };
          }
          
          // Clear success message after 3 seconds
          setTimeout(() => this.clearAllMessages(), 3000);
        },
        error: (error) => {
          console.error('Error updating phone:', error);
          this.phoneError = error.message || 'Failed to update phone number';
          this.isLoading = false;
        }
      });
  }

  // Delete account
  deleteAccount(): void {
    if (!this.currentUser?.username) return;

    this.isLoading = true;
    this.clearAllMessages();

    this.userService.deleteUser(this.currentUser.username, this.currentUser.username)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          // Logout user and redirect to landing page
          this.authService.logout();
          this.router.navigate(['/']); // Redirect to main landing page
        },
        error: (error) => {
          console.error('Error deleting account:', error);
          this.error = error.message || 'Failed to delete account';
          this.isLoading = false;
        }
      });
  }

  // Navigation methods - Updated for admin users
  navigateToHome(): void {
    if (this.isAdmin) {
      this.router.navigate(['/admin-home']);
    } else {
      this.router.navigate(['/home']);
    }
  }

  navigateToExpenses(): void {
    this.router.navigate(['/expenses']);
  }

  navigateToReports(): void {
    this.router.navigate(['/reports']);
  }

  navigateToAdminHome(): void {
    this.router.navigate(['/admin-home']);
  }

  // Logout
  logout(): void {
    this.authService.logout();
  }

  // Utility methods
  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      control?.markAsTouched();
    });
  }

  public clearAllMessages(): void {
    this.error = '';
    this.successMessage = '';
    this.currentPasswordError = '';
    this.newPasswordError = '';
    this.phoneError = '';
  }

  // Form field helpers
  isFieldInvalid(formGroup: FormGroup, fieldName: string): boolean {
    const field = formGroup.get(fieldName);
    return !!(field && field.invalid && field.touched);
  }

  getFieldError(formGroup: FormGroup, fieldName: string): string {
    const field = formGroup.get(fieldName);
    
    if (field?.errors) {
      if (field.errors['required']) return `This field is required`;
      if (field.errors['minlength']) return `Must be at least ${field.errors['minlength'].requiredLength} characters`;
      if (field.errors['pattern']) return `Please enter a valid ${fieldName}`;
      if (field.errors['passwordMismatch']) return 'Passwords do not match';
    }
    
    return '';
  }
}