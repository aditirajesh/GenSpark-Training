import { Component, OnInit } from '@angular/core';
import { User } from '../models/UserModel';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { UserService } from '../services/user.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-dashboard',
  imports: [FormsModule, ReactiveFormsModule,CommonModule,FormsModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})
export class DashboardComponent implements OnInit {
  users: User[] = [];
  filteredUsers: User[] = [];
  chartData: ChartData = {
    gender: { female: 0, male: 0 },
    role: {},
    state: {}
  };
  
  addUserForm: FormGroup;
  showAddForm = false;
  
  // Filter properties
  filterText = '';
  selectedGender = '';
  selectedRole = '';
  selectedState = '';
  
  // Available options for dropdowns
  roles = ['Developer', 'Designer', 'Manager', 'Admin', 'HR'];
  states = [
    'Alabama', 'Alaska', 'Arizona', 'Arkansas', 'California', 'Colorado',
    'Connecticut', 'Delaware', 'Florida', 'Georgia', 'Hawaii', 'Idaho',
    'Illinois', 'Indiana', 'Iowa', 'Kansas', 'Kentucky', 'Louisiana',
    'Maine', 'Maryland', 'Massachusetts', 'Michigan', 'Minnesota',
    'Mississippi', 'Missouri', 'Montana', 'Nebraska', 'Nevada',
    'New Hampshire', 'New Jersey', 'New Mexico', 'New York',
    'North Carolina', 'North Dakota', 'Ohio', 'Oklahoma', 'Oregon',
    'Pennsylvania', 'Rhode Island', 'South Carolina', 'South Dakota',
    'Tennessee', 'Texas', 'Utah', 'Vermont', 'Virginia', 'Washington',
    'West Virginia', 'Wisconsin', 'Wyoming'
  ];

  constructor(
    private userService: UserService,
    private fb: FormBuilder
  ) {
    this.addUserForm = this.fb.group({
      firstName: ['', Validators.required],
      gender: ['', Validators.required],
      role: ['', Validators.required],
      state: ['', Validators.required],
    });
  }

  ngOnInit() {
    this.loadUsers();
  }

  loadUsers() {
    this.userService.getAllUsers().subscribe({
      next: (response) => {

        this.users = response.users.map((user: any) => ({
          id: user.id,
          firstName: user.firstName,
          gender: user.gender,
          role: this.getRandomRole(),
          state: this.getRandomState(),
        }));
        
        this.filteredUsers = [...this.users];
        this.updateChartData();
        this.userService.updateUsersList(this.users);
      },
      error: (error) => {
        console.error('Error loading users:', error);
      }
    });
  }

  getRandomRole(): string {
    return this.roles[Math.floor(Math.random() * this.roles.length)];
  }

  getRandomState(): string {
    return this.states[Math.floor(Math.random() * this.states.length)];
  }

  onAddUser() {
    if (this.addUserForm.valid) {
      const newUser = this.addUserForm.value;
      
      this.userService.addUser(newUser).subscribe({
        next: (response) => {
          console.log('User added successfully:', response);
          this.users.unshift({ ...newUser, id: response.id });
          this.applyFilters();
          this.updateChartData();
          this.addUserForm.reset();
          this.showAddForm = false;
        },
        error: (error) => {
          console.error('Error adding user:', error);
        }
      });
    }
  }

  applyFilters() {
    this.filteredUsers = this.users.filter(user => {
      const matchesText = !this.filterText || 
        user.firstName.toLowerCase().includes(this.filterText.toLowerCase())      
      const matchesGender = !this.selectedGender || user.gender === this.selectedGender;
      const matchesRole = !this.selectedRole || user.role === this.selectedRole;
      const matchesState = !this.selectedState || user.state === this.selectedState;
      
      return matchesText && matchesGender && matchesRole && matchesState;
    });
    
    this.updateChartData();
  }

  clearFilters() {
    this.filterText = '';
    this.selectedGender = '';
    this.selectedRole = '';
    this.selectedState = '';
    this.applyFilters();
  }

  updateChartData() {
    this.chartData = {
      gender: { female: 0, male: 0 },
      role: {},
      state: {}
    };

    this.filteredUsers.forEach(user => {
      if (user.gender.toLowerCase() === 'female') {
        this.chartData.gender.female++;
      } else if (user.gender.toLowerCase() === 'male') {
        this.chartData.gender.male++;
      }

      this.chartData.role[user.role] = (this.chartData.role[user.role] || 0) + 1;

      this.chartData.state[user.state] = (this.chartData.state[user.state] || 0) + 1;
    });
  }

  getGenderPercentage(gender: 'female' | 'male'): number {
    const total = this.chartData.gender.female + this.chartData.gender.male;
    return total > 0 ? Math.round((this.chartData.gender[gender] / total) * 100) : 0;
  }

  getRoleEntries(): Array<{key: string, value: number}> {
    return Object.entries(this.chartData.role).map(([key, value]) => ({key, value}));
  }

  getTopStates(limit: number = 10): Array<{key: string, value: number}> {
    return Object.entries(this.chartData.state)
      .sort(([,a], [,b]) => b - a)
      .slice(0, limit)
      .map(([key, value]) => ({key, value}));
  }

  getMaxRoleCount(): number {
    return Math.max(...Object.values(this.chartData.role), 1);
  }
}
