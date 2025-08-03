function doGet() {
  const sheet = SpreadsheetApp.getActiveSpreadsheet().getActiveSheet();
  const data = sheet.getDataRange().getValues();
  if (data.length === 0) {
    return ContentService.createTextOutput(JSON.stringify({ lists: [] }))
      .setMimeType(ContentService.MimeType.JSON);
  }
  const headers = data.shift().map(h => h.toString().trim());
  const listIdx = headers.indexOf('List');
  const itemIdx = headers.indexOf('Item');
  const qtyIdx = headers.indexOf('Units');
  const posIdx = headers.indexOf('Position');
  const completedIdx = headers.indexOf('Completed');
  const lists = {};
  data.forEach(row => {
    const listName = listIdx >= 0 ? row[listIdx] : 'List';
    const itemName = itemIdx >= 0 ? row[itemIdx] : '';
    if (!itemName) return;
    const quantity = qtyIdx >= 0 ? row[qtyIdx] : 0;
    const position = posIdx >= 0 ? row[posIdx] : -1;
    const completed = completedIdx >= 0 ? row[completedIdx] : false;
    if (!lists[listName]) {
      lists[listName] = { name: listName, items: [] };
    }
    lists[listName].items.push({ name: itemName, quantity: quantity, position: position, completed: completed });
  });
  return ContentService.createTextOutput(JSON.stringify({ lists: Object.values(lists) }))
    .setMimeType(ContentService.MimeType.JSON);
}

function doPost(e) {
  const data = JSON.parse(e.postData.contents);
  const sheet = SpreadsheetApp.getActiveSpreadsheet().getActiveSheet();
  sheet.clear();
  sheet.appendRow(['List', 'Item', 'Units', 'Position', 'Completed']);
  data.lists.forEach(function(list) {
    list.items.forEach(function(item, index) {
      const pos = (item.position !== undefined && item.position !== null) ? item.position : index;
      const completed = item.completed === true || item.completed === 'true';
      sheet.appendRow([list.name, item.name, item.quantity, pos, completed]);
    });
  });
  return ContentService.createTextOutput('OK');
}
