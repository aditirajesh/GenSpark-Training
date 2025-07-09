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

  constructor(private expenseService: ExpenseService) {}

  ngOnInit(): void {
    this.initializeForm();
  }

  private initializeForm(): void {
    if (this.expense) {
      this.editForm = {
        title: this.expense.title,
        amount: this.expense.amount,
        category: this.expense.category,
        notes: this.expense.notes || ''
      };
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
      this.editForm.category
    );
  }

  onSubmit(): void {
    if (!this.isFormValid() || this.isSubmitting) {
      return;
    }

    this.isSubmitting = true;
    this.error = '';

    const updateDto: ExpenseUpdateRequestDto = {
      id: this.expense.id,
      title: this.editForm.title.trim(),
      amount: this.editForm.amount,
      category: this.editForm.category,
      notes: this.editForm.notes.trim() || undefined,
      receipt: this.selectedFile || undefined
    };

    console.log('Updating expense:', {
      ...updateDto,
      receipt: this.selectedFile ? `${this.selectedFile.name} (${this.selectedFile.size} bytes)` : 'No new receipt',
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
            owner: updatedExpense.username
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