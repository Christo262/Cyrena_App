export interface ModelReview {
  category: 'Offline' | 'Cloud';
  model: string;
  provider: string;
  size?: string;
  bestUseCases: string[];
  thinkingMode: string;
  strengths: string[];
  weaknesses: string[];
  rating: number;
  detailedReview: string;
  proTip?: string;
}
