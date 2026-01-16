export function parseMarkdown(text: string): string {
  if (!text) return '';

  // 1. Escape HTML
  let safeText = text
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;")
    .replace(/"/g, "&quot;")
    .replace(/'/g, "&#039;");

  // 2. Code blocks
  const codeBlocks: string[] = [];
  safeText = safeText.replace(/```([\s\S]*?)```/g, (match: string, code: string) => {
    codeBlocks.push(`<pre><code>${code}</code></pre>`);
    return `__CODE_BLOCK_${codeBlocks.length - 1}__`;
  });

  // 3. Inline code
  safeText = safeText.replace(/`([^`]+)`/g, '<code>$1</code>');

  // 4. Bold
  safeText = safeText.replace(/\*\*([^*]+)\*\*/g, '<strong>$1</strong>');

  // 5. Italic
  safeText = safeText.replace(/\*([^*]+)\*/g, '<em>$1</em>');

  // 6. Lists (simple replacement for lines starting with - or *)
  // We will handle lists by checking lines.
  const lines = safeText.split('\n');
  let inList = false;
  const processedLines: string[] = [];

  for (const line of lines) {
    if (line.trim().match(/^[-*]\s+(.*)/)) {
      if (!inList) {
        processedLines.push('<ul>');
        inList = true;
      }
      processedLines.push(line.replace(/^[-*]\s+(.*)/, '<li>$1</li>'));
    } else {
      if (inList) {
        processedLines.push('</ul>');
        inList = false;
      }
      processedLines.push(line);
    }
  }
  if (inList) {
    processedLines.push('</ul>');
  }

  safeText = processedLines.join('\n');

  // 7. Newlines to <br> (only if not inside a list or pre tag logic handled implicitly?)
  // We joined by \n. Now replace \n with <br>, but we need to be careful about the <ul> and <li> structure we just created.
  // Actually, inside <ul> we don't want <br> between <li>.
  // And we don't want <br> around the <ul> tags.

  // Let's replace \n with <br> but remove <br> around block tags.
  safeText = safeText.replace(/\n/g, '<br>');
  safeText = safeText.replace(/<ul><br>/g, '<ul>');
  safeText = safeText.replace(/<\/ul><br>/g, '</ul>');
  safeText = safeText.replace(/<\/li><br>/g, '</li>');

  // 8. Restore code blocks
  safeText = safeText.replace(/__CODE_BLOCK_(\d+)__/g, (match: string, index: string) => {
    return codeBlocks[parseInt(index)] || '';
  });

  return safeText;
}
