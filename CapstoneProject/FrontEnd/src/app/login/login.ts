import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, AbstractControl } from '@angular/forms';
import { AuthLayoutComponent } from '../auth-layout/auth-layout';
import { AuthService } from '../services/auth.service';
import { Router } from '@angular/router';
import { UserLoginRequestDto } from '../models/AuthDTOs';

@Component({
  selector: 'app-login',
  imports: [CommonModule, ReactiveFormsModule, AuthLayoutComponent],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class LoginComponent implements OnInit {
  
  loginForm: FormGroup;
  loading = false;
  errorMessage = '';
  successMessage = '';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.loginForm = this.fb.group({
      email: ['', [
        Validators.required,
        this.emailValidator
      ]],
      password: ['', [
        Validators.required,
        this.passwordValidator
      ]],
    });
  }

  emailValidator(control: AbstractControl) {
    if (!control.value) return null;
    const emailRegex = /^[^@\s]+@[^@\s]+\.[^@\s]+$/;
    return emailRegex.test(control.value) ? null : { invalidEmail: true };
  }

  passwordValidator(control: AbstractControl) {
    if (!control.value) return null;
    const password = control.value;
    const errors: any = {};

    if (password.length < 6) errors.minLength = true;
    if (!/[A-Z]/.test(password)) errors.uppercase = true;
    if (!/[0-9]/.test(password)) errors.number = true;
    if (!/[\W_]/.test(password)) errors.specialChar = true;

    return Object.keys(errors).length > 0 ? { invalidPassword: errors } : null;
  }

  ngOnInit(): void {
    const navigation = this.router.getCurrentNavigation();
    const email = navigation?.extras.state?.['email'];
    
    if (email) {
      this.loginForm.patchValue({ email });
    }
  }

  onLogin(): void {
    if (this.loginForm.invalid) {
      this.markFormGroupTouched();
      return;
    }

    this.loading = true;
    this.clearMessages();

    const credentials: UserLoginRequestDto = {
      username: this.loginForm.value.email,
      password: this.loginForm.value.password
    };

    this.authService.login(credentials).subscribe({
      next: (response) => {
        this.loading = false;
        this.successMessage = `Welcome back, ${response.username}!`;
        
        setTimeout(() => {
          this.router.navigate(['/home']);
        }, 1000);
      },
      error: (error) => {
        this.loading = false;
        this.errorMessage = error.message || 'Login failed. Please check your credentials.';
      }
    });
  }

  navigateToSignup(): void {
    this.router.navigate(['/signup']);
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.loginForm.get(fieldName);
    return !!(field && field.invalid && field.touched);
  }

  getFieldErrorMessage(fieldName: string): string {
    const field = this.loginForm.get(fieldName);
    if (!field || !field.errors || !field.touched) return '';

    if (field.errors['required']) {
      return fieldName === 'email' ? 'Email address is required' : 'Password is required';
    }

    if (field.errors['invalidEmail']) {
      return 'Username must be a valid email address format (e.g., user@example.com)';
    }

    if (field.errors['invalidPassword']) {
      return 'Password must be at least 6 characters long, and contain at least one uppercase letter, one number, and one special character';
    }

    return 'Please enter a valid value';
  }

  private markFormGroupTouched(): void {
    Object.keys(this.loginForm.controls).forEach(key => {
      this.loginForm.get(key)?.markAsTouched();
    });
  }

  private clearMessages(): void {
    this.errorMessage = '';
    this.successMessage = '';
  }
}