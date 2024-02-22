import { Component } from '@angular/core';
import { AuthService } from '../auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  email: string = '';
  password: string = '';

  constructor(private authService: AuthService, private router: Router) {}

  login(): void {
    this.authService.login(this.email, this.password).subscribe({
      next: () => {
        // Redirect to home page or any other desired page upon successful login
        this.router.navigateByUrl('/home');
      },
      error: (error) => {
        console.error('Login failed:', error);
        // Handle login error (e.g., display error message to the user)
      }
    });
  }
}