import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="home-container">
      <div class="hero-section">
        <div class="hero-content">
          <h1 class="hero-title">🚀 Sistema de Gestión OpenUP</h1>
          <p class="hero-subtitle">
            Gestiona tus proyectos con la metodología OpenUP de forma eficiente
          </p>
        </div>
      </div>

      <div class="features-grid">
        <div class="feature-card" routerLink="/projects">
          <div class="feature-icon">📁</div>
          <h3>Proyectos</h3>
          <p>Administra y visualiza todos tus proyectos OpenUP con sus fases</p>
          <button class="feature-btn">Ver Proyectos</button>
        </div>

        <div class="feature-card" routerLink="/planning">
          <div class="feature-icon">📊</div>
          <h3>Planning e Iteraciones</h3>
          <p>Define iteraciones, gestiona tareas y da seguimiento al progreso</p>
          <button class="feature-btn">Ir al Planning</button>
        </div>

        <div class="feature-card" routerLink="/microincrements">
          <div class="feature-icon">📈</div>
          <h3>Microincrementos</h3>
          <p>Registra y monitorea los microincrementos de tus entregas</p>
          <button class="feature-btn">Ver Microincrementos</button>
        </div>
      </div>

      <div class="quick-actions">
        <h2>Acciones Rápidas</h2>
        <div class="actions-row">
          <a routerLink="/projects/new" class="action-link primary">
            ➕ Crear Nuevo Proyecto
          </a>
          <a routerLink="/microincrements/new" class="action-link secondary">
            📝 Registrar Microincremento
          </a>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .home-container {
      max-width: 1200px;
      margin: 0 auto;
      padding: 2rem;
    }

    .hero-section {
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      border-radius: 20px;
      padding: 4rem 2rem;
      text-align: center;
      color: white;
      margin-bottom: 3rem;
      box-shadow: 0 10px 30px rgba(0,0,0,0.1);
    }

    .hero-title {
      font-size: 3rem;
      font-weight: 700;
      margin-bottom: 1rem;
      text-shadow: 2px 2px 4px rgba(0,0,0,0.2);
    }

    .hero-subtitle {
      font-size: 1.3rem;
      opacity: 0.95;
      max-width: 600px;
      margin: 0 auto;
    }

    .features-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
      gap: 2rem;
      margin-bottom: 3rem;
    }

    .feature-card {
      background: white;
      border-radius: 16px;
      padding: 2rem;
      text-align: center;
      box-shadow: 0 4px 12px rgba(0,0,0,0.08);
      transition: all 0.3s ease;
      cursor: pointer;
      border: 2px solid transparent;
    }

    .feature-card:hover {
      transform: translateY(-8px);
      box-shadow: 0 12px 24px rgba(0,0,0,0.15);
      border-color: #667eea;
    }

    .feature-icon {
      font-size: 4rem;
      margin-bottom: 1rem;
    }

    .feature-card h3 {
      font-size: 1.5rem;
      color: #333;
      margin-bottom: 0.5rem;
    }

    .feature-card p {
      color: #666;
      margin-bottom: 1.5rem;
      line-height: 1.6;
    }

    .feature-btn {
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      color: white;
      border: none;
      padding: 0.75rem 2rem;
      border-radius: 25px;
      font-size: 1rem;
      font-weight: 600;
      cursor: pointer;
      transition: all 0.3s ease;
    }

    .feature-btn:hover {
      transform: scale(1.05);
      box-shadow: 0 4px 12px rgba(102, 126, 234, 0.4);
    }

    .quick-actions {
      background: #f8f9fa;
      border-radius: 16px;
      padding: 2rem;
      text-align: center;
    }

    .quick-actions h2 {
      color: #333;
      margin-bottom: 1.5rem;
      font-size: 1.8rem;
    }

    .actions-row {
      display: flex;
      gap: 1rem;
      justify-content: center;
      flex-wrap: wrap;
    }

    .action-link {
      padding: 1rem 2.5rem;
      border-radius: 30px;
      text-decoration: none;
      font-weight: 600;
      font-size: 1.1rem;
      transition: all 0.3s ease;
      display: inline-block;
    }

    .action-link.primary {
      background: linear-gradient(135deg, #11998e 0%, #38ef7d 100%);
      color: white;
    }

    .action-link.primary:hover {
      transform: translateY(-3px);
      box-shadow: 0 8px 16px rgba(17, 153, 142, 0.3);
    }

    .action-link.secondary {
      background: white;
      color: #667eea;
      border: 2px solid #667eea;
    }

    .action-link.secondary:hover {
      background: #667eea;
      color: white;
      transform: translateY(-3px);
      box-shadow: 0 8px 16px rgba(102, 126, 234, 0.3);
    }

    @media (max-width: 768px) {
      .hero-title {
        font-size: 2rem;
      }

      .features-grid {
        grid-template-columns: 1fr;
      }

      .actions-row {
        flex-direction: column;
      }

      .action-link {
        width: 100%;
      }
    }
  `]
})
export class HomeComponent {}