import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProductsService } from '../../services/products.service';
import { ProductResponse } from '../../models/product-response';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { MatCardModule } from '@angular/material/card';
import { RouterModule } from '@angular/router';
import { UsersService } from '../../services/users.service';

@Component({
  selector: 'app-show-case',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatDividerModule, MatButtonModule, RouterModule],
  templateUrl: './show-case.component.html',
  styleUrl: './show-case.component.css'
})
export class ShowCaseComponent {
  products: ProductResponse[] = [];

  constructor(private productsService: ProductsService, public usersService: UsersService) {
  }

  ngOnInit(): void {
    this.productsService.getProducts().subscribe({
      next: (response: ProductResponse[]) => {
        this.products = response;
      },

      error: (err) => {
        console.log(err);
      }
    });
  }
}
