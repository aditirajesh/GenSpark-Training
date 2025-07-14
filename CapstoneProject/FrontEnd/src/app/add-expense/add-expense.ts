import { Component, EventEmitter, Input, Output } from '@angular/core';
import { Expense } from '../models/ExpenseModel';
import { ExpenseService } from '../services/expense.service';
import { Router } from '@angular/router';
import { ExpenseAddRequestDto } from '../models/ExpenseDTOs';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-add-expense',
  imports: [CommonModule, FormsModule],
  templateUrl: './add-expense.html',
  styleUrl: './add-expense.css'
})
export class AddExpenseComponent {
  @Output() expenseAdded = new EventEmitter<Expense>();
  @Output() closeModal = new EventEmitter<void>();
  
  // NEW: Admin context inputs
  @Input() targetUsername: string | null = null;
  @Input() adminMode: boolean = false;
  @Input() viewingUserDisplayName: string = '';

  expenseForm = {
    title: '',
    amount: 0,
    category: '',
    notes: ''
  };

  selectedFile: File | null = null;
  isSubmitting = false;
  error = '';

  categories = [
    'Food & Dining',
    'Transportation', 
    'Shopping',
    'Entertainment',
    'Health & Medical',
    'Bills & Utilities',
    'Travel',
    'Education',
    'Personal Care',
    'Gifts & Donations'
  ];

  constructor(
    private expenseService: ExpenseService,
    private router: Router
  ) {}

  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      // Validate file size (5MB limit)
      const maxSize = 5 * 1024 * 1024; // 5MB
      if (file.size > maxSize) {
        this.error = 'File size must be less than 5MB';
        return;
      }
      
      const allowedTypes = ['image/jpeg', 'image/png', 'image/gif', 'application/pdf'];
      if (!allowedTypes.includes(file.type)) {
        this.error = 'Only images (JPEG, PNG, GIF) and PDF files are allowed';
        return;
      }
      
      this.selectedFile = file;
      this.error = '';
    }
  }

  isFormValid(): boolean {
    return !!(
      this.expenseForm.title?.trim() &&
      this.expenseForm.amount > 0 &&
      this.expenseForm.category
    );
  }

  onSubmit(): void {
    if (!this.isFormValid() || this.isSubmitting) {
      return;
    }

    this.isSubmitting = true;
    this.error = '';

    const addDto: ExpenseAddRequestDto = {
      title: this.expenseForm.title.trim(),
      amount: this.expenseForm.amount,
      category: this.expenseForm.category,
      notes: this.expenseForm.notes.trim(),
      receiptBill: this.selectedFile || undefined,
      // 🎯 CRITICAL: Include targetUsername when admin is creating for another user
      targetUsername: this.adminMode && this.targetUsername ? this.targetUsername : undefined
    };

    console.log('Creating expense:', {
      ...addDto,
      receiptBill: this.selectedFile ? `${this.selectedFile.name} (${this.selectedFile.size} bytes)` : 'None',
      adminMode: this.adminMode,
      targetUsername: this.targetUsername,
      actualTargetInDto: addDto.targetUsername
    });

    this.expenseService.addExpense(addDto).subscribe({
      next: (newExpense) => {
        console.log('Expense created successfully:', newExpense);
        
        // 🔍 Verify the expense was created for the correct user
        if (this.adminMode && this.targetUsername && newExpense.username !== this.targetUsername) {
          console.error('ERROR: Expense created for wrong user!', {
            expected: this.targetUsername,
            actual: newExpense.username,
            expenseId: newExpense.id,
            expenseTitle: newExpense.title
          });
          this.error = `Error: Expense was created for ${newExpense.username} instead of ${this.targetUsername}`;
          this.isSubmitting = false;
          return;
        }
        
        // Success - expense created for correct user
        if (this.adminMode && this.targetUsername) {
          console.log(`✅ SUCCESS: Admin expense created for ${this.targetUsername}`, {
            expenseId: newExpense.id,
            title: newExpense.title,
            amount: newExpense.amount,
            owner: newExpense.username
          });
        }
        
        this.expenseAdded.emit(newExpense);
        this.resetForm();
        this.isSubmitting = false;
      },
      error: (error) => {
        console.error('Error creating expense:', error);
        this.error = error.message || 'Failed to add expense. Please try again.';
        this.isSubmitting = false;
      }
    });
  }

  onCancel(): void {
    this.closeModal.emit();
  }

  private resetForm(): void {
    this.expenseForm = {
      title: '',
      amount: 0,
      category: '',
      notes: ''
    };
    this.selectedFile = null;
    this.error = '';
  }

  // Computed properties for UI
  get modalTitle(): string {
    if (this.adminMode && this.viewingUserDisplayName) {
      return `Add Expense for ${this.viewingUserDisplayName}`;
    }
    return 'Add New Expense';
  }

  get submitButtonText(): string {
    if (this.isSubmitting) {
      return this.adminMode ? 'Creating...' : 'Creating...';
    }
    return this.adminMode && this.viewingUserDisplayName 
      ? `Create for ${this.viewingUserDisplayName}` 
      : 'Add Expense';
  }

  get userContextMessage(): string {
    if (this.adminMode && this.viewingUserDisplayName) {
      return `This expense will be created for ${this.viewingUserDisplayName}`;
    }
    return '';
  }

  get placeholderText(): string {
    if (this.adminMode && this.viewingUserDisplayName) {
      return `Enter expense title for ${this.viewingUserDisplayName}`;
    }
    return 'Enter expense title';
  }
}