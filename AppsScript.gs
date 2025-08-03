function doGet() {
  const sheet = SpreadsheetApp.getActiveSpreadsheet().getActiveSheet();
  const data = sheet.getDataRange().getValues();
  const headers = data.shift();
  const lists = {};
  data.forEach(row => {
    const listName = row[0];
    const itemName = row[1];
    const quantity = row[2];
    const position = row[3];
    if (!lists[listName]) {
      lists[listName] = { name: listName, items: [] };
    }
    lists[listName].items.push({ name: itemName, quantity: quantity, position: position });
  });
  return ContentService.createTextOutput(JSON.stringify({ lists: Object.values(lists) }))
    .setMimeType(ContentService.MimeType.JSON);
}

function doPost(e) {
  const data = JSON.parse(e.postData.contents);
  const sheet = SpreadsheetApp.getActiveSpreadsheet().getActiveSheet();
  sheet.clear();
  sheet.appendRow(['List', 'Item', 'Units', 'Position']);
  data.lists.forEach(list => {
    list.items.forEach(item => {
      sheet.appendRow([list.name, item.name, item.quantity, item.position]);
    });
  });
  return ContentService.createTextOutput('OK');
}
