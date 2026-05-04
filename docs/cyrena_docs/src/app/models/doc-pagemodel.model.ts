export interface DocPage {
  id: string;
  title: string;
  description: string;
  route: string;
  icon?: string;
  children?: DocPage[];
}

export interface DocSection {
  heading: string;
  content: string;
  code?: string;
}

export interface FaqItem {
  question: string;
  answer: string;
}
