const MANAGER_URL = '<REPLACE_WITH_MANAGER_ENDPOINT>';

function getListsFromSheet() {
  const sheet = SpreadsheetApp.getActiveSpreadsheet().getActiveSheet();
  const data = sheet.getDataRange().getValues();
  if (data.length === 0) {
    return { lists: [] };
  }
  const headers = data.shift().map(h => h.toString().trim());
  const idIdx = headers.indexOf('Id');
  const listIdx = headers.indexOf('List');
  const itemIdx = headers.indexOf('Item');
  const qtyIdx = headers.indexOf('Units');
  const posIdx = headers.indexOf('Position');
  const completedIdx = headers.indexOf('Completed');
  const lists = {};
  data.forEach(row => {
    const listName = listIdx >= 0 ? row[listIdx] : 'List';
    const itemName = itemIdx >= 0 ? row[itemIdx] : '';
    const id = idIdx >= 0 ? row[idIdx] : '';
    if (!itemName) return;
    const quantity = qtyIdx >= 0 ? row[qtyIdx] : 0;
    const position = posIdx >= 0 ? row[posIdx] : -1;
    const rawCompleted = completedIdx >= 0 ? row[completedIdx] : false;
    const completed = typeof rawCompleted === 'string'
      ? rawCompleted.toLowerCase() === 'true'
      : rawCompleted === true;
    if (!lists[listName]) {
      lists[listName] = { name: listName, items: [] };
    }
    lists[listName].items.push({ id: id, name: itemName, quantity: quantity, position: position, completed: completed });
  });
  return { lists: Object.values(lists) };
}

function writeListsToSheet(data) {
  const sheet = SpreadsheetApp.getActiveSpreadsheet().getActiveSheet();
  sheet.clear();
  sheet.appendRow(['Id', 'List', 'Item', 'Units', 'Position', 'Completed']);
  data.lists.forEach(function(list) {
    list.items.forEach(function(item, index) {
      const pos = (item.position !== undefined && item.position !== null) ? item.position : index;
      const completed = item.completed === true || item.completed === 'true';
      sheet.appendRow([item.id || '', list.name, item.name, item.quantity, pos, completed ? 'true' : 'false']);
    });
  });
}

function doGet() {
  return ContentService.createTextOutput(JSON.stringify(getListsFromSheet()))
    .setMimeType(ContentService.MimeType.JSON);
}

function doPost(e) {
  const data = JSON.parse(e.postData.contents);
  setSyncing(true);
  writeListsToSheet(data);
  setSyncing(false);
  return ContentService.createTextOutput('OK');
}

function onEdit(e) {
  if (isSyncing()) return;
  syncToManager();
}

function syncToManager() {
  const payload = JSON.stringify(getListsFromSheet());
  UrlFetchApp.fetch(MANAGER_URL, {
    method: 'post',
    contentType: 'application/json',
    payload: payload,
    muteHttpExceptions: true
  });
}

function isSyncing() {
  return PropertiesService.getScriptProperties().getProperty('SYNCING') === '1';
}

function setSyncing(flag) {
  PropertiesService.getScriptProperties().setProperty('SYNCING', flag ? '1' : '0');
}

