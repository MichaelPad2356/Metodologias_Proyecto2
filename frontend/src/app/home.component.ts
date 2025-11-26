import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div style="text-align: center; margin-top: 20px;">
       <h1>Bienvenido a Metodologías</h1>
       <p>Esta es tu vista actual conservada.</p>
       
       <a href="/planning" style="background: blue; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;">
         Ir al Planning (HU-015/016)
       </a>
    </div>
  `
})
export class HomeComponent {}