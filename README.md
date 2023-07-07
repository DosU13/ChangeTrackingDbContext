Usage:
1) Extend your DbContext class with ChangeTrackingDbContext instead of DbContext:
```
   public class LocalDBContext: ChangeTrackingDbContext
```
2) Mark your Table with `[ChangeTrackable]` annotation:
```
[ChangeTrackable]
[Table("Employee")]
public class Employee
{
...
}
```
 3) Whenever you make changes (Delete, Insert, Update) to `[ChangeTrackable]` marked entities, use `SaveChanges(string username, string ipAddress)` instead of SaveChanges() or it will throw Exception:
```
using(var db = new LocalDB())
{
    db.Employees.Remove(employeeToRemove);
    var emp = db.Employees.First(x => x.id == 13);
    emp.firstname = "Batman";
    db.Employees.Add(newEmployee);
    db.SaveChanges("DosU", "10.10.5.27");
}
```

Than it will save changes to a new Table named `ChangeTrack`:
<table>
  <tr>
<td>Id</td>	<td>table_name</td>	<td>username</td>	<td>ipaddress</td>	<td>change_date</td>	<td>operation</td>	<td>old_values</td>	<td>new_values</td>
  </tr>
  <tr>
<td>1</td>	<td>Employee</td>	<td>DosU</td>	<td>10.10.5.27</td>	<td>2023-07-07 14:11:03.400</td>	<td>1</td>	<td>NULL</td>	<td>{"id":"11","lastname":"Bond","firstname":"James","birthdate":"11/11/1920"}</td>
  </tr>
  <tr>
<td>2</td>	<td>Employee</td>	<td>DosU</td>	<td>10.10.5.27</td>	<td>2023-07-07 14:11:03.463</td>	<td>2</td>	<td>{"id":"13","lastname":"Wayne","firstname":"Bruce","birthdate":"03/30/1988"}</td><td>{"id":"201","lastname":"Wayne","firstname":"Batman","birthdate":"8/7/1994 12:00:00 AM"}</td>
  </tr>
  <tr>
<td>3</td>	<td>Employee</td>	<td>DosU</td>	<td>10.10.5.27</td> <td>2023-07-07 14:11:03.463</td>	<td>0</td>	<td>{"id":"12","lastname":"Newman","firstname":"Bill","birthdate":"01/01/2000"}</td>	<td>NULL</td>
  </tr>
</table>

operation:
<ul>
  <li>0 -> Delete</li>
  <li>1 -> Insert</li>
  <li>2 -> Update</li>
</ul>
