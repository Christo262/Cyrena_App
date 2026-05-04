export interface PackageVersion {
  version: string;
  sizeBytes: number;
  contentHash: string | null;
  createdAt: string;
  releaseNotes: string | null;
}

export interface Package {
  id: string;
  packageId: string;
  title: string | null;
  description: string | null;
  supportedOperatingSystems: string[];
  supportedArchitectures: string[];
  hasIcon: boolean;
  latestVersion: string | null;
  versions: PackageVersion[];
}
