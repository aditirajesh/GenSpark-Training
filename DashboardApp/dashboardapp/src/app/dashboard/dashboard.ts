import { Component, OnInit } from '@angular/core';
import { User } from '../models/UserModel';
import { ChartData } from '../models/ChartModel';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { UserService } from '../services/user.service';
import { AgCharts } from 'ag-charts-angular';
import { AgChartOptions } from 'ag-charts-community';


@Component({
  selector: 'app-dashboard',
  imports: [FormsModule,ReactiveFormsModule,CommonModule,AgCharts],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})
export class Dashboard implements OnInit{
  users: User[] = [];
  filtered_users: User[] = [];
  chart_data: ChartData = {
    gender: { female: 0, male: 0 },
    role: {},
    state: {}};
  add_form: FormGroup;

  filtered_text = '';
  selected_gender = '';
  selected_role = '';
  selected_state = '';

  constructor(
    private userService: UserService,
    private fb: FormBuilder
  ) {
    this.add_form = this.fb.group({
      firstName: ['',Validators.required],
      gender: ['',Validators.required],
      state: ['',Validators.required],
      role: ['',Validators.required]
    })
  }

  ngOnInit() {
    this.loadUsers();
  }

  loadUsers() {
    this.userService.getAllUsers().subscribe({
      next:(response) => {
        this.users = response.users.map((user:any) => ({
          id: user.id,
          firstName: user.firstName,
          gender: user.gender,
          role: user.role,
          state: user.state
        }));

        this.filtered_users = [...this.users];
        this.userService.updateUser(this.users);

      }, error :(error) => {  
        console.error('Error loading users:', error);
      }
    })

  }

  onAddUser() {
    if (this.add_form.valid) {
      const newUser = this.add_form.value;
      this.userService.addUser(newUser).subscribe({
        next:(response) => {
          this.users.unshift({...newUser, id: response.id});
          this.add_form.reset();
      }, error: (error) => {
          console.error('Error adding user:', error);
        }
      });
    }
  }

  filterUsers() {
    this.filtered_users = this.users.filter(user => {
      const name_match = !this.filtered_text || user.firstName.toLowerCase().includes(this.filtered_text.toLowerCase());
      const gender_match = !this.selected_gender || user.gender.toLowerCase() === this.selected_gender.toLowerCase();
      const role_match = !this.selected_role || user.role.toLowerCase() === this.selected_role.toLowerCase();
      const state_match = !this.selected_state || user.state.toLowerCase() === this.selected_state.toLowerCase();

      return name_match && gender_match && role_match && state_match;
    });
  }

  clearFilters() {
    this.filtered_text='';
    this.selected_gender = '';
    this.selected_role = '';
    this.selected_state = '';
  }

  ChartStats() {
    this.chart_data = {
      gender:{female:0,male:0},
      role:{},
      state:{}
    }

    this.filtered_users.forEach(user => {
      if (user.gender.toLowerCase() === 'female') {
        this.chart_data.gender.female++;
      } else if (user.gender.toLowerCase() === 'male') {
        this.chart_data.gender.male++
      }

    if (this.chart_data.role[user.role.toLowerCase()]) {
      this.chart_data.role[user.role.toLowerCase()]++
    } else {
      this.chart_data.role[user.role.toLowerCase()] = 1;
    }

    if (this.chart_data.state[user.state.toLowerCase()]) {
      this.chart_data.role[user.state.toLowerCase()]++
    } else {
      this.chart_data.role[user.role.toLowerCase()] = 1;
    }
  });
}
}
