import { Component, ChangeDetectionStrategy, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ModelReviewsServiceService } from '../../services/model-reviews-service.service';
import { ModelReview } from '../../models/model-reviewmodel.model';

@Component({
  selector: 'app-model-reviews-page',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './model-reviews-page.component.html',
  styleUrl: './model-reviews-page.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ModelReviewsPageComponent {
  private readonly service = inject(ModelReviewsServiceService);

  readonly offlineReviews = signal<ModelReview[]>(this.service.getOfflineReviews());
  readonly cloudReviews = signal<ModelReview[]>(this.service.getCloudReviews());
  readonly allReviews = computed(() => [...this.offlineReviews(), ...this.cloudReviews()]);

  readonly selectedCategory = signal<string>('All');

  readonly filteredReviews = computed(() => {
    const category = this.selectedCategory();
    if (category === 'All') return this.allReviews();
    return this.allReviews().filter(r => r.category === category);
  });

  setCategory(category: string): void {
    this.selectedCategory.set(category);
  }

  trackByModel(index: number, review: ModelReview): string {
    return review.model;
  }
}
