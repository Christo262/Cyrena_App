import { ChangeDetectionStrategy, Component, signal } from '@angular/core';
import { FaqItem } from '../../../../models/doc-pagemodel.model';

@Component({
  selector: 'app-faq',
  standalone: true,
  imports: [],
  templateUrl: './faq.component.html',
  styleUrl: './faq.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class FaqComponent {
  readonly faqs = signal<FaqItem[]>([
    {
      question: 'What is Cyréna?',
      answer: 'Cyréna is an AI platform that provides developers with powerful tools to build, deploy, and scale intelligent applications. It offers APIs, SDKs, and a suite of features including conversational AI, custom agents, knowledge bases, and tool integrations.'
    },
    {
      question: 'How do I get started?',
      answer: 'Getting started is easy! Create a free account, generate an API key from your dashboard, install the SDK for your preferred language, and make your first API request. Check out our Getting Started guide for a detailed walkthrough.'
    },
    {
      question: 'Is there a free tier?',
      answer: 'Yes! Cyréna offers a generous free tier that includes access to all core features with rate limits suitable for development and small projects. Upgrade to a paid plan when you\'re ready to scale.'
    },
    {
      question: 'What programming languages are supported?',
      answer: 'Cyréna provides official SDKs for JavaScript/TypeScript, Python, Go, and Ruby. Our REST API can be used with any language that supports HTTP requests.'
    },
    {
      question: 'How is pricing calculated?',
      answer: 'Pricing is based on token usage. You only pay for what you use. Each plan includes a base amount of tokens, with overage charged at a per-token rate. Visit our pricing page for detailed information.'
    },
    {
      question: 'Is my data secure?',
      answer: 'Absolutely. Cyréna implements enterprise-grade security including end-to-end encryption, SOC 2 compliance, and strict data handling policies. Your data is never used to train models without explicit consent.'
    },
    {
      question: 'Can I use Cyréna in production?',
      answer: 'Yes, Cyréna is built for production workloads. We offer SLA guarantees, dedicated support, and infrastructure designed for high availability and low latency at scale.'
    },
    {
      question: 'How do I get support?',
      answer: 'Free tier users have access to community support via Discord and GitHub discussions. Paid plans include priority email support and dedicated account managers for enterprise customers.'
    }
  ]);

  readonly openIndex = signal<number | null>(null);

  toggle(index: number): void {
    this.openIndex.update(current => current === index ? null : index);
  }
}
