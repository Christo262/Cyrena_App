import { ChangeDetectionStrategy, Component } from '@angular/core';

interface DevelopPlanMember {
  member: string;
  description: string;
}

interface VersionControlMethod {
  category: string;
  method: string;
  description: string;
}

interface FileExtensionMethod {
  method: string;
  signature: string;
  purpose: string;
}

interface FolderExtensionMethod {
  method: string;
  signature: string;
  purpose: string;
}

interface DevelopOptionConstant {
  constant: string;
  value: string;
  purpose: string;
}

interface CoreDependency {
  type: string;
  source: string;
  usage: string;
}

@Component({
  selector: 'app-cyrena-coding-core',
  standalone: true,
  imports: [],
  templateUrl: './cyrena-coding-core.component.html',
  styleUrl: './cyrena-coding-core.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CyrenaCodingCoreComponent {
  readonly developPlanMembers: DevelopPlanMember[] = [
    { member: 'Plan', description: 'Current DevelopPlan instance' },
    { member: 'SetPlan(DevelopPlan)', description: 'Replaces the plan (project switching)' },
    { member: 'OnDevelopPlanChanged', description: 'Subscribe to plan replacement events' },
    { member: 'OnFileCreated / OnFileUpdated / OnFileDeleted', description: 'Subscribe to file lifecycle events' },
    { member: 'InvokeFileCreated / InvokeFileUpdated / InvokeFileDeleted', description: 'Raise events from plugins/services' }
  ];

  readonly versionControlMethods: VersionControlMethod[] = [
    { category: 'Configuration', method: 'MaxVersionsPerFile', description: 'Max snapshots per file (default: 20)' },
    { category: 'Write', method: 'Backup(DevelopFileContent?, string?)', description: 'Create a versioned snapshot with optional label' },
    { category: 'Write', method: 'RemoveBackup(string)', description: 'Remove all backups for a file ID' },
    { category: 'Write', method: 'Clear()', description: 'Remove all backups across all files' },
    { category: 'Query', method: 'HasBackup(string)', description: 'Check if any backup exists for file ID' },
    { category: 'Query', method: 'GetLatest(string)', description: 'Get most recent backup for file ID' },
    { category: 'Query', method: 'GetHistory(string)', description: 'Get all versioned snapshots for file ID' },
    { category: 'Query', method: 'GetAllLatest()', description: 'Get latest snapshot for every backed-up file' },
    { category: 'Restore', method: 'TryGetVersion(string, int, out)', description: 'Get backup by index (0 = oldest)' },
    { category: 'Restore', method: 'TryGetVersionAt(string, DateTimeOffset, out)', description: 'Get backup closest to timestamp' },
    { category: 'Rollback', method: 'RollbackTo(DevelopFileVersion)', description: 'Restore to a specific version snapshot' },
    { category: 'Rollback', method: 'RollbackOne(string)', description: 'Restore to the previous version' },
    { category: 'Shim', method: 'GetBackups(string)', description: 'Backward-compatible: returns latest content' },
    { category: 'Shim', method: 'GetBackups()', description: 'Backward-compatible: returns all latest contents' }
  ];

  readonly fileExtensionMethods: FileExtensionMethod[] = [
    { method: 'CreateFile', signature: '(plan, fileId, fileName, content) → DevelopFile', purpose: 'Create file in root. Idempotent by fileId.' },
    { method: 'CreateFile', signature: '(plan, folder, fileId, fileName, content) → DevelopFile', purpose: 'Create file in folder. Idempotent by fileId.' },
    { method: 'TryReadFileContent', signature: '(plan, file, out content) → bool', purpose: 'Read full content. Returns false without mutating plan if missing.' },
    { method: 'TryReadFileLines', signature: '(plan, file, out lines) → bool', purpose: 'Read as line dictionary. Same non-mutating failure behavior.' },
    { method: 'TryWriteFileContent', signature: '(plan, file, content, out fileContent) → bool', purpose: 'Overwrite file content on disk.' },
    { method: 'TryWriteFileLine', signature: '(plan, file, index, line, out lines) → bool', purpose: 'Replace single line at index. Validates bounds.' },
    { method: 'TryInsertLine', signature: '(plan, file, index, line, out lines) → bool', purpose: 'Insert line at index. Shifts subsequent lines down.' },
    { method: 'TryInsertLines', signature: '(plan, file, afterIndex, newLines, out lines) → bool', purpose: 'Insert multiple lines from end backward to preserve indices.' },
    { method: 'TryReplaceLines', signature: '(plan, file, startIndex, count, replacement, out lines) → bool', purpose: 'Replace a block of lines. count clamped to available lines.' },
    { method: 'RemoveFile', signature: '(plan, file) → bool', purpose: 'Delete file from disk and remove from plan (root or nested).' },
    { method: 'TryFindFile', signature: '(plan, fileId, out file, recursive) → bool', purpose: 'Find by ID. Recursive by default.' },
    { method: 'TryFindFile', signature: '(plan, folder, fileId, out file, recursive) → bool', purpose: 'Find by ID within folder subtree.' },
    { method: 'TryFindFileByName', signature: '(plan, name, out file, recursive) → bool', purpose: 'Find by name (case-insensitive).' },
    { method: 'TryFindFileByName', signature: '(plan, folder, name, out file, recursive) → bool', purpose: 'Find by name within folder subtree.' },
    { method: 'IndexFiles', signature: '(plan, extension, id_prefix, readOnly)', purpose: 'Auto-index root files by extension. Suffix-only strip.' },
    { method: 'IndexFiles', signature: '(plan, folder, extension, id_prefix, readOnly)', purpose: 'Auto-index folder files. Same stripping logic.' }
  ];

  readonly folderExtensionMethods: FolderExtensionMethod[] = [
    { method: 'CreateFolder', signature: '(plan, id, name) → DevelopFolder', purpose: 'Create folder in root. Idempotent by ID.' },
    { method: 'CreateFolder', signature: '(plan, parent, id, name) → DevelopFolder', purpose: 'Create nested folder. Uses parent.RelativePath for disk path.' },
    { method: 'RemoveFolder', signature: '(plan, folder, recursive) → bool', purpose: 'Delete folder from disk and plan.' },
    { method: 'TryFindFolder', signature: '(plan, folderId, out folder, recursive) → bool', purpose: 'Find by ID.' },
    { method: 'TryFindFolder', signature: '(folder, folderId, out model, recursive) → bool', purpose: 'Find by ID within folder subtree.' },
    { method: 'GetFolderOfFile', signature: '(plan, file) → DevelopFolder?', purpose: 'Find containing folder (root-level search).' },
    { method: 'GetFolderOfFile', signature: '(plan, folder, file) → DevelopFolder?', purpose: 'Find containing folder within subtree.' },
    { method: 'GetOrCreateFolder', signature: '(plan, id, name) → DevelopFolder', purpose: 'Get existing or create in root.' },
    { method: 'GetOrCreateFolder', signature: '(plan, parent, id, name) → DevelopFolder', purpose: 'Get existing or create nested. Searches within parent only.' }
  ];

  readonly developOptionConstants: DevelopOptionConstant[] = [
    { constant: 'AssistantModeId', value: '"developer"', purpose: 'ID for the developer assistant mode' },
    { constant: 'BuilderId', value: '"dev.builder-id"', purpose: 'Key storing the selected ICodeBuilder.Id' },
    { constant: 'RootDirectory', value: '"dev.root-dir"', purpose: 'Key storing the project root directory path' }
  ];

  readonly coreDependencies: CoreDependency[] = [
    { type: 'Entity', source: 'Cyrena', usage: 'Base for DevelopItem, StickyNote' },
    { type: 'IJsonSerializable', source: 'Cyrena', usage: 'Implemented by DevelopItem, DevelopPlan' },
    { type: 'ChatConfiguration', source: 'Cyrena', usage: 'Passed to ICodeBuilder.DeleteAsync / EditAsync' },
    { type: 'CyrenaKernelBuilder', source: 'Cyrena', usage: 'Passed to ICodeBuilder.ConfigureAsync' },
    { type: 'ToolResult / ToolResult<T>', source: 'Cyrena', usage: 'Return type for Semantic Kernel plugin functions' }
  ];
}
