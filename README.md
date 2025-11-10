# Coffee-Filter
A small programming language made for fun, includes a parser, grammar, tokenizer, AST walker and a command prompt made in C#  
  
Try the release to test the language in a small command prompt.  
_______
Language Grammar:
|program|line* EOF|
|---|---|
|**line →**|declaration / statement|
|**declaration →**|variable_declaration_statement / function_declaration_statement|
|**variable_declaration_statement →**|"var" IDENTIFIER("=" expression)?";"|
|**function_declaration_statement →**|"fun" function|
|**function →**|IDENTIFIER "("parameters?")" block_statement|
|**parameters →**|IDENTIFIER ( "," IDENTIFIER )*|
|**statement →**|expression_statement / for_statement / if_statement / while_statement / block_statement / return_statement|
|**for_statement →**|"for" "(" ( variable_declaration_statement / expression_statement / ";" ) expression? ";" expression? ")" statement|
|**while_statement →**|"while" "(" expression ")" statement|
|**if_statement →**|"if" "(" expression ")" statement ( "else" statement )?|
|**block_statement →**|"{" line* "}"|
|**expression_statement →**|expression ";"|
|**return_statement →**|"return" expression?";"  |
|**expression →**|assignment|
|**assignment →**|(IDENTIFIER "=" assignment) | logic_or|
|**logic_or →**|logic_and ("or" logic_and)*
|**logic_and →**|equality ("and" equality)*|
|**equality →**|comparison ( ( "!=" / "==" ) comparison )*|
|**comparison →**|term ( ( ">" / ">=" / "<" / "<=" ) term )*|
|**term →**|factor ( ( "-" / "+" ) factor )*|
|**factor →**|unary ( ( "/" / "*" ) unary )*|
|**unary →**|( "!" / "-" ) unary / function_call|
|**function_call →**|object_expression / ("(" arguments?")")*|
|**object_expression →**|{"declaration*"}" / primary|
|**primary →**|NUMBER / STRING / "true" / "false" / "nil" / "(" expression ")" / IDENTIFIER / "[" arguments? "]"|
