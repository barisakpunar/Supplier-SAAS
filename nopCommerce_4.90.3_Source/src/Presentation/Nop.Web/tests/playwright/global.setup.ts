import { execFileSync } from 'node:child_process';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import type { FullConfig } from '@playwright/test';

export default async function globalSetup(_config: FullConfig): Promise<void> {
  const currentDir = path.dirname(fileURLToPath(import.meta.url));
  const rootDir = path.resolve(currentDir, '../../../../../..');
  const applyScript = path.join(rootDir, 'scripts/dealer-finance-fixture/apply.sh');
  const verifyScript = path.join(rootDir, 'scripts/dealer-finance-fixture/verify.sh');

  execFileSync('bash', [applyScript], { stdio: 'inherit' });
  execFileSync('bash', [verifyScript], { stdio: 'inherit' });
}
