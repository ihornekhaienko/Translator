# Translator
Simple Compiler & Imterpeter written in C#.

## Available operations
Currently available basic math operations, i/o to console, if statement and for loops.

## Grammar
grammar PL17;

### Parser rules
main : 'main' DO_SECTION ;

DO_SECTION : '{' STATEMENT_LIST '}' ;
STATEMENT_LIST : STATEMENT ';' (STATEMENT ';')*? ;
STATEMENT : DECLARATION | ASSIGN_STATEMENT | OUTPUT | FOR_STATEMENT | IF_STATEMENT ;

DECLARATION : TYPE | IDENT_LIST ( ASSIGN )? ;
IDENT_LIST : ID ( ',' ID)*? ;
ASSIGN : '=' ( BOOL_EXPRESSION ) ;

ASSIGN_STATEMENT : IDENT_LIST ASSIGN ;

BOOL_EXPRESSION : ADD_EXPRESSION ( LOGIC_OP ADD_EXPRESSION )? ;
ADD_EXPRESSION : MULT_EXPRESSION ( ADD_OP MULT_EXPRESSION )? ;
MULT_EXPRESSION : POW_EXPRESSION ( MULT_OP POW_EXPRESSION )? ;
POW_EXPRESSION : SIGNED_FACTOR ( POW_OP SIGNED_FACTOR )? ;
SIGNED_FACTOR : ( '-' )? FACTOR ;
FACTOR : ARITH_CONST | ID | '(' BOOL_EXPRESSION ')' | INPUT;

INPUT : 'read' ;

OUTPUT : 'write' OUTPUT_LIST ;
OUTPUT_LIST : BOOL_EXPRESSION ( ',' BOOL_EXPRESSION )*? ;

FOR_STATEMENT : 'for' INDEX 'by' STEP 'while' CONDITION DO_SECTION ;
INDEX : ID ASSIGN ;
STEP : ADD_EXPRESSION ;
CONDITION : BOOL_EXPRESSION ;

IF_STATEMENT : 'if' BOOL_EXPRESSION 'then' DO_SECTION 'fi' ( 'else' DO_SECTION ) ;

### Lexer rules
fragment DIGIT : [0-9] ;
fragment LETTER : [a-zA-Z] ;

fragment INT: DIGIT+ ;
fragment FLOAT : DIGIT+ ([.] DIGIT+)? ;
fragment BOOL : 'true' | 'false' ;
fragment TEXT : '\'' .*? '\'' ;

fragment TYPE : 'int' | 'float' | 'text' | 'bool' ;

fragment ARITH_CONST : INT | FLOAT;

ID : LETTER ( LETTER | DIGIT | '_')*? ;
CONST : ARITH_CONST | BOOL | TEXT;

KEYWORD : 'main' | 'for' | 'by' | 'while' | 'do' | 'if' | 'then' | 'fi' | 'else' |
		   'write' | TYPE ;

ADD_OP : '+' | '-' ; 
MULT_OP : '*' | '/' | '%';
POW_OP : '^' ;

BRACKETS : [(){}] ;

PUNCT : [,;] ;

ASSIGN_OP : '=' ;

LOGIC_OP : '==' | '!=' | '>' | '<' | '>=' | '<=' ;

WS : [ \t\r\n]+ -> skip;
