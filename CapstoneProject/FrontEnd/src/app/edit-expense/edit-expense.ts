import { Component, EventEmitter, Input, Output } from '@angular/core';
import { Expense } from '../models/ExpenseModel';
import { ExpenseService } from '../services/expense.service';
import { ExpenseUpdateRequestDto } from '../models/ExpenseDTOs';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-edit-expense',
  imports: [CommonModule, FormsModule],
  templateUrl: './edit-expense.html',
  styleUrl: './edit-expense.css'
})
export class EditExpenseComponent {
  @Input() expense!: Expense;
  @Output() expenseUpdated = new EventEmitter<Expense>();
  @Output() closeModal = new EventEmitter<void>();
  
  // NEW: Admin context inputs
  @Input() targetUsername: string | null = null;
  @Input() adminMode: boolean = false;
  @Input() viewingUserDisplayName: string = '';

  editForm = {
    title: '',
    amount: 0,
    category: '',
    notes: '',
    expenseDate: ''  // NEW: Expense date field
  };

  selectedFile: File | null = null;
  isSubmitting = false;
  error = '';
  dateError = '';  // NEW: Date-specific error messages
  maxDate = '';    // NEW: Maximum allowed date (today)

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

  constructor(private expenseService: ExpenseService) {
    // NEW: Set max date to today
    this.maxDate = this.getTodayDateString();
  }

  ngOnInit(): void {
    this.initializeForm();
  }

  // NEW: Get today's date in YYYY-MM-DD format
  private getTodayDateString(): string {
    const today = new Date();
    return today.toISOString().split('T')[0];
  }

  // NEW: Convert Date to YYYY-MM-DD format for input
  private formatDateForInput(date: Date | string): string {
    if (!date) return '';
    
    try {
      const dateObj = date instanceof Date ? date : new Date(date);
      if (isNaN(dateObj.getTime())) return '';
      
      return dateObj.toISOString().split('T')[0];
    } catch (error) {
      return '';
    }
  }

  private initializeForm(): void {
    if (this.expense) {
      this.editForm = {
        title: this.expense.title,
        amount: this.expense.amount,
        category: this.expense.category,
        notes: this.expense.notes || '',
        expenseDate: this.formatDateForInput(this.expense.createdAt)  // NEW: Set current expense date
      };
    }
  }

  // NEW: Validate expense date
  onExpenseDateChange(): void {
    this.dateError = '';
    
    if (this.editForm.expenseDate) {
      const selectedDate = new Date(this.editForm.expenseDate);
      const today = new Date();
      today.setHours(23, 59, 59, 999); // Set to end of today
      
      if (selectedDate > today) {
        this.dateError = 'Expense date cannot be in the future';
        return;
      }
      
      // Optional: Check if date is too far in the past (e.g., more than 2 years)
      const twoYearsAgo = new Date();
      twoYearsAgo.setFullYear(today.getFullYear() - 2);
      
      if (selectedDate < twoYearsAgo) {
        this.dateError = 'Expense date cannot be more than 2 years in the past';
        return;
      }
    }
  }

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
      this.editForm.title?.trim() &&
      this.editForm.amount > 0 &&
      this.editForm.category &&
      !this.dateError  // NEW: Include date validation
    );
  }

  onSubmit(): void {
    if (!this.isFormValid() || this.isSubmitting) {
      return;
    }

    // NEW: Final date validation before submission
    if (this.editForm.expenseDate) {
      const selectedDate = new Date(this.editForm.expenseDate);
      const today = new Date();
      today.setHours(23, 59, 59, 999);
      
      if (selectedDate > today) {
        this.dateError = 'Expense date cannot be in the future';
        return;
      }
    }

    this.isSubmitting = true;
    this.error = '';

    // FIXED: Handle expense date properly - convert to ISO string for FormData
    let expenseDateString: string | undefined = undefined;
    if (this.editForm.expenseDate) {
      // Create date object and set time to noon to avoid timezone issues
      const expenseDate = new Date(this.editForm.expenseDate + 'T12:00:00');
      expenseDateString = expenseDate.toISOString();
    }

    const updateDto: ExpenseUpdateRequestDto = {
      id: this.expense.id,
      title: this.editForm.title.trim(),
      amount: this.editForm.amount,
      category: this.editForm.category,
      notes: this.editForm.notes.trim() || undefined,
      receipt: this.selectedFile || undefined,
      ExpenseDate: expenseDateString  // FIXED: Use ExpenseDate (capital E) to match backend exactly
    };

    console.log('Updating expense:', {
      ...updateDto,
      receipt: this.selectedFile ? `${this.selectedFile.name} (${this.selectedFile.size} bytes)` : 'No new receipt',
      ExpenseDate: expenseDateString || 'Not changed',
      adminMode: this.adminMode,
      targetUsername: this.targetUsername,
      originalOwner: this.expense.username
    });

    this.expenseService.updateExpense(updateDto).subscribe({
      next: (updatedExpense) => {
        console.log('Expense updated successfully:', updatedExpense);
        
        // 🔍 Verify the expense still belongs to the correct user
        if (updatedExpense.username !== this.expense.username) {
          console.error('ERROR: Expense ownership changed during update!', {
            original: this.expense.username,
            updated: updatedExpense.username,
            expenseId: updatedExpense.id
          });
          this.error = 'Error: Expense ownership was changed unexpectedly';
          this.isSubmitting = false;
          return;
        }
        
        // Success - expense updated correctly
        if (this.adminMode && this.targetUsername) {
          console.log(`✅ SUCCESS: Admin updated expense for ${this.targetUsername}`, {
            expenseId: updatedExpense.id,
            title: updatedExpense.title,
            amount: updatedExpense.amount,
            owner: updatedExpense.username,
            expenseDate: updatedExpense.createdAt
          });
        }
        
        this.expenseUpdated.emit(updatedExpense);
        this.isSubmitting = false;
      },
      error: (error) => {
        console.error('Error updating expense:', error);
        this.error = error.message || 'Failed to update expense. Please try again.';
        this.isSubmitting = false;
      }
    });
  }

  onCancel(): void {
    this.closeModal.emit();
  }

  // NEW: Get current expense date display text
  getCurrentExpenseDateDisplay(): string {
    if (!this.expense?.createdAt) return 'Unknown';
    
    try {
      const date = new Date(this.expense.createdAt);
      return date.toLocaleDateString('en-GB', {
        day: 'numeric',
        month: 'short',
        year: 'numeric'
      });
    } catch (error) {
      return 'Invalid Date';
    }
  }

  // Computed properties for UI
  get modalTitle(): string {
    if (this.adminMode && this.viewingUserDisplayName) {
      return `Edit ${this.viewingUserDisplayName}'s Expense`;
    }
    return 'Edit Expense';
  }

  get submitButtonText(): string {
    if (this.isSubmitting) {
      return 'Updating...';
    }
    return this.adminMode && this.viewingUserDisplayName 
      ? `Update for ${this.viewingUserDisplayName}` 
      : 'Update Expense';
  }

  get userContextMessage(): string {
    if (this.adminMode && this.viewingUserDisplayName) {
      return `Editing expense for ${this.viewingUserDisplayName}`;
    }
    return '';
  }

  get hasCurrentReceipt(): boolean {
    return !!(this.expense?.receipt?.id);
  }

  get currentReceiptName(): string {
    return this.expense?.receipt?.receiptName || 'Receipt';
  }

  get placeholderText(): string {
    if (this.adminMode && this.viewingUserDisplayName) {
      return `Enter expense title for ${this.viewingUserDisplayName}`;
    }
    return 'Enter expense title';
  }
}