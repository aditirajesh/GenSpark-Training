import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, AbstractControl } from '@angular/forms';
import { AuthLayoutComponent } from '../auth-layout/auth-layout';
import { UserAddRequestDto } from '../models/UserDTOs';
import { UserService } from '../services/user.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-signup',
  imports: [CommonModule, ReactiveFormsModule, AuthLayoutComponent],
  templateUrl: './signup.html',
  styleUrl: './signup.css'
})
export class SignupComponent {
  
  signupForm: FormGroup;
  loading = false;
  errorMessage = '';
  successMessage = '';

  constructor(
    private fb: FormBuilder,
    private userService: UserService,
    private router: Router
  ) {
    this.signupForm = this.fb.group({
      email: ['', [
        Validators.required,
        this.emailValidator
      ]],
      phone: ['', [
        Validators.required,
        this.phoneValidator
      ]],
      password: ['', [
        Validators.required,
        this.passwordValidator
      ]],
      confirmPassword: ['', [Validators.required]]
    }, { validators: this.passwordMatchValidator });
  }

  emailValidator(control: AbstractControl) {
    if (!control.value) return null;
    const emailRegex = /^[^@\s]+@[^@\s]+\.[^@\s]+$/;
    return emailRegex.test(control.value) ? null : { invalidEmail: true };
  }

  phoneValidator(control: AbstractControl) {
    if (!control.value) return null;
    const phone = control.value.toString();
    return (phone.length === 10 && /^\d{10}$/.test(phone)) ? null : { invalidPhone: true };
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

  passwordMatchValidator(form: AbstractControl) {
    const password = form.get('password');
    const confirmPassword = form.get('confirmPassword');
    
    if (password && confirmPassword && password.value !== confirmPassword.value) {
      confirmPassword.setErrors({ passwordMismatch: true });
    } else {
      const errors = confirmPassword?.errors;
      if (errors) {
        delete errors['passwordMismatch'];
        confirmPassword?.setErrors(Object.keys(errors).length === 0 ? null : errors);
      }
    }
    return null;
  }

  onSignup(): void {
    if (this.signupForm.invalid) {
      this.markFormGroupTouched();
      return;
    }

    this.loading = true;
    this.clearMessages();

    const signupData: UserAddRequestDto = {
      username: this.signupForm.value.email,
      password: this.signupForm.value.password,
      role: 'User',
      phone: this.signupForm.value.phone
    };

    this.userService.createUser(signupData).subscribe({
      next: (response) => {
        this.loading = false;
        this.successMessage = 'Account created successfully! Redirecting to login...';
        
        setTimeout(() => {
          this.router.navigate(['/login'], {
            state: { email: this.signupForm.value.email }
          });
        }, 2000);
      },
      error: (error) => {
        this.loading = false;
        this.errorMessage = error.message || 'Failed to create account. Please try again.';
      }
    });
  }

  navigateToLogin(): void {
    this.router.navigate(['/login']);
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.signupForm.get(fieldName);
    return !!(field && field.invalid && field.touched);
  }

  getFieldErrorMessage(fieldName: string): string {
    const field = this.signupForm.get(fieldName);
    if (!field || !field.errors || !field.touched) return '';

    if (field.errors['required']) {
      switch (fieldName) {
        case 'email': return 'Email address is required';
        case 'phone': return 'Phone number is required';
        case 'password': return 'Password is required';
        case 'confirmPassword': return 'Please confirm your password';
        default: return 'This field is required';
      }
    }

    if (field.errors['invalidEmail']) {
      return 'Username must be a valid email address format (e.g., user@example.com)';
    }

    if (field.errors['invalidPhone']) {
      return 'Phone must be exactly 10 digits';
    }

    if (field.errors['invalidPassword']) {
      return 'Password must be at least 6 characters long, and contain at least one uppercase letter, one number, and one special character';
    }

    if (field.errors['passwordMismatch']) {
      return 'Passwords must match';
    }

    return 'Please enter a valid value';
  }

  private markFormGroupTouched(): void {
    Object.keys(this.signupForm.controls).forEach(key => {
      this.signupForm.get(key)?.markAsTouched();
    });
  }

  private clearMessages(): void {
    this.errorMessage = '';
    this.successMessage = '';
  }
}