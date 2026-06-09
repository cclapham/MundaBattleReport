// Extract gang data from Munda Manager iframe
window.extractGangDataFromIframe = function() {
    try {
        const iframe = document.getElementById('munda-iframe');
        if (!iframe || !iframe.contentDocument) {
            return null;
        }

        const doc = iframe.contentDocument;
        const fighters = [];

        // Munda Manager wraps each fighter in an <a> tag with href containing '/fighter/'
        const fighterLinks = doc.querySelectorAll('a[href*="/fighter/"]');

        if (fighterLinks.length === 0) {
            return null;
        }

        fighterLinks.forEach(link => {
            const card = link.querySelector('div[class*="fighter-card-bg"]');
            if (!card) return;

            const nameEl = card.querySelector('.fancy-print-keep-color-heading');
            const fighterName = nameEl?.textContent?.trim() || '';

            const typeEl = card.querySelector('.fancy-print-keep-color-subtitle');
            const fighterTypeRaw = typeEl?.textContent?.trim() || 'Ganger';

            const typeMatch = fighterTypeRaw.match(/(.+?)\s*\((.+?)\)/);
            const fighterClass = typeMatch ? typeMatch[1].trim() : '';
            const fighterType = typeMatch ? typeMatch[2].trim() : fighterTypeRaw;

            let credits = 0;
            const creditSpans = Array.from(card.querySelectorAll('span')).find(el => el.textContent?.includes('Credits'));
            if (creditSpans) {
                const creditValue = creditSpans.previousElementSibling?.textContent?.trim() || '';
                credits = parseInt(creditValue) || 0;
            }

            const statsTable = card.querySelector('table:not(.table-weapons)');
            const stats = extractStatsFromTable(statsTable);

            const weaponsTable = card.querySelector('table.table-weapons');
            const weapons = extractWeaponsFromTable(weaponsTable);

            const wargearText = card.innerText.match(/Wargear\s+(.+?)(?=Skills|$)/)?.[1]?.trim() || '';
            const skillsText = card.innerText.match(/Skills\s+(.+?)(?=Special|Wargear|$)/)?.[1]?.trim() || '';

            if (fighterName) {
                fighters.push({
                    id: link.getAttribute('href')?.replace('/fighter/', '') || '',
                    fighterName: fighterName,
                    fighterType: fighterType,
                    fighterClass: fighterClass,
                    credits: credits,
                    movement: stats.m || 0,
                    weaponSkill: stats.ws || 0,
                    ballisticSkill: stats.bs || 0,
                    strength: stats.s || 0,
                    toughness: stats.t || 0,
                    wounds: stats.w || 0,
                    initiative: stats.i || 0,
                    attacks: stats.a || 0,
                    leadership: stats.ld || 0,
                    cool: stats.cl || 0,
                    willpower: stats.wil || 0,
                    intelligence: stats.int || 0,
                    weapons: weapons,
                    wargear: wargearText.split(',').map(w => ({ name: w.trim() })).filter(w => w.name),
                    effects: { active: skillsText.split(',').map(s => s.trim()).filter(s => s) }
                });
            }
        });

        if (fighters.length === 0) {
            return null;
        }

        return JSON.stringify(fighters);
    }
    catch (error) {
        console.error('Error extracting gang data:', error);
        return null;
    }
};

function extractStatsFromTable(table) {
    if (!table) return {};
    const stats = {};
    const headerCells = table.querySelectorAll('thead th');
    const valueCells = table.querySelectorAll('tbody td');

    headerCells.forEach((cell, idx) => {
        const label = cell.textContent?.trim().toLowerCase() || '';
        const value = valueCells[idx]?.textContent?.trim() || '';
        if (label === 'm') stats.m = parseInt(value) || 0;
        if (label === 'ws') stats.ws = parseInt(value) || 0;
        if (label === 'bs') stats.bs = parseInt(value) || 0;
        if (label === 's') stats.s = parseInt(value) || 0;
        if (label === 't') stats.t = parseInt(value) || 0;
        if (label === 'w') stats.w = parseInt(value) || 0;
        if (label === 'i') stats.i = parseInt(value) || 0;
        if (label === 'a') stats.a = parseInt(value) || 0;
        if (label === 'ld') stats.ld = parseInt(value) || 0;
        if (label === 'cl') stats.cl = parseInt(value) || 0;
        if (label === 'wil') stats.wil = parseInt(value) || 0;
        if (label === 'int') stats.int = parseInt(value) || 0;
    });

    return stats;
}

function extractWeaponsFromTable(table) {
    if (!table) return [];
    const weapons = [];
    const rows = table.querySelectorAll('tbody tr');

    rows.forEach(row => {
        const nameCell = row.querySelector('td:first-child');
        if (nameCell) {
            weapons.push({
                name: nameCell.textContent?.trim() || 'Unknown Weapon',
                type: 'ranged'
            });
        }
    });

    return weapons;
}
