import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { AuthGuard } from './auth-guard';

describe('AuthGuard', () => {
  let guard: AuthGuard;
  let router: Router;
  let routerSpy: jasmine.Spy;

  beforeEach(() => {
    const routerSpyObj = jasmine.createSpyObj('Router', ['navigate']);

    TestBed.configureTestingModule({
      providers: [
        AuthGuard,
        { provide: Router, useValue: routerSpyObj }
      ]
    });

    guard = TestBed.inject(AuthGuard);
    router = TestBed.inject(Router);
    routerSpy = router.navigate as jasmine.Spy;
  });

  afterEach(() => {
    // Clean up localStorage after each test
    localStorage.clear();
  });

  it('should be created', () => {
    expect(guard).toBeTruthy();
  });

  it('should allow activation when token exists', () => {
    // Arrange
    localStorage.setItem('token', 'fake-token');
    
    // Act
    const result = guard.canActivate({} as any, {} as any);
    
    // Assert
    expect(result).toBe(true);
    expect(routerSpy).not.toHaveBeenCalled();
  });

  it('should deny activation and redirect to login when token does not exist', () => {
    // Arrange
    localStorage.removeItem('token');
    
    // Act
    const result = guard.canActivate({} as any, {} as any);
    
    // Assert
    expect(result).toBe(false);
    expect(routerSpy).toHaveBeenCalledWith(['login']);
  });

  it('should deny activation and redirect to login when token is null', () => {
    // Arrange
    localStorage.setItem('token', '');
    localStorage.removeItem('token'); // Ensures it's null
    
    // Act
    const result = guard.canActivate({} as any, {} as any);
    
    // Assert
    expect(result).toBe(false);
    expect(routerSpy).toHaveBeenCalledWith(['login']);
  });
});