export interface PaginationQuery {
  pageNumber: number;         
  pageSize: number;             
}

export interface PaginatedResponse<T> {
  items: T[];                  
  totalCount: number;          
  pageNumber: number;         
  pageSize: number;           
  totalPages: number;           
}