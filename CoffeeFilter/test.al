//variable declaration coffee filter supports Int, Bool, Fun, String, List, Object and Null 
var hello = 0;
hello = 10;
hello = 100 * hello / 2;

//print is a standard function
print(hello + 10);

//you can define Objects like this
var hej = {
	var x = 100;
};

//you can access the values with dot syntax
hej.x = 10;
print(hej.x+1);
print("asd " + 2);

//Lists are supported
print("List Behavior");
var list = [1, 2, "hej", false];
for(var i=0;i<list.size;i=i+1)
{
	print(list[i]);
}

list.add("hello");
for(var i=0;i<list.size;i=i+1)
{
	print(list[i]);
}