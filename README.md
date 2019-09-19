
# DataTable Comparer

A .NET Standard 2.0 class library that allows for the comparison of the data and structure in two or more System.DataTable objects, with a focus on data comparison. Table structures do not have to be identical, but they need to have the same primary key structure.

## Example

In the example below are two tables. The row 2 First_Name is different (Table->"Beatice", Table_1->"Samone").
In the Rest table (Datatabe Comparer) the *_`Out of sync`_* in row 2 indicates the differnce between the two rows.

Table

| ID |First_Name    | Last_Name  |
| --- |:-------------| ----------:|
| 1 | Joe          | Smith      |
| 2 | Beatrice     | Smith      |
| 3 | James        | Bond       |

Table_1

| ID |First_Name    | Last_Name  |
| --- |:-------------| ----------:|
| 1 | Joe          | Smith      |
| 2 | Samone       | Smith      |
| 3 | James        | Bond       |


Datatable Comparer Result

| ID | Exists In Table | Exists In Table_1 | Exists In Status | First_Name Status | Last_Name Status | First_Name_Table | First_Name_Table_1 | Last_Name_Table | Last_Name_Table_1 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | Yes | Yes | In sync | In sync | In sync | Joe | Joe | Smith | Smith |
| 2 | Yes | Yes | In sync | **_`Out of sync`_** | In sync | Beatrice | Samone | Smith | Smith |
| 3 | Yes | Yes | In sync | In sync | In sync | James | James | Bond | Bond |

Here is another example with three tables, table 3 has extra row

Table

| ID |First_Name    | Last_Name  |
| --- |:-------------| ----------:|
| 1 | Joe          | Smith      |
| 2 | Samone       | Smith      |
| 3 | James        | Bond       |

Table_1

| ID |First_Name    | Last_Name  |
| --- |:-------------| ----------:|
| 1 | Joe          | Smith      |
| 2 | Samone       | Smith      |
| 3 | James        | Bond       |

Table_2

| ID |First_Name    | Last_Name  |
| --- |:-------------| ----------:|
| 1 | Joe          | Smith      |
| 2 | Samone       | Smith      |
| 3 | James        | Bond       |
| 4 | Joe          | Bond       |

Datatable Comparer Result

| ID   |Exists_In_Table|Exists_In_Table_1|Exists_In_Table_2|Exists_In_Status|FIRST_NAME_Status|LAST_NAME_Status|First_Name_Table|First_Name_Table_1|First_Name_Table_2|Last_Name_Table|Last_Name_Table_1|Last_Name_Table_2|
| ---  |:--------------| ---------------:|----------------:| --------------:| ---------------:| --------------:| --------------:| ----------------:| ----------------:| -------------:| ---------------:| ---------------:|
| 1    | Yes  | Yes | Yes| In sync    | In sync| In sync|    Joe|    Joe|    Joe| Smith| Smith| Smith|
| 2    | Yes  | Yes | Yes| In sync    | In sync| In sync| Samone| Samone| Samone| Smith| Smith| Smith|
| 3    | Yes  | Yes | Yes| In sync    | In sync| In sync|  James|  James|  James|  Bond|  Bond|  Bond|
| 4    |      |     | Yes| Out of sync|        |        |       |       |    Joe|      |      |  Bond|
