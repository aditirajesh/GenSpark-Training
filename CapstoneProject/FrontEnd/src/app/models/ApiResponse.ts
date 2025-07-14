export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  message?: string;
  errors?: string[];
}

// For handling errors consistently
export interface ApiError {
  message: string;
  status: number;
  details?: any;
}