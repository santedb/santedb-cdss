grammar CdssLibrary;

options { caseInsensitive = true; }

library: 
    (include_definition)*  
    library_definition 
    EOF;

include_definition: INCLUDE(SPACE)?(NAMED_ID|STRING);
library_definition: (DEFINE)? LIBRARY STRING
    (having_statements)*
    (metadata_statement)?
    AS
    (library_definitions)+
    END (LIBRARY)?;

having_statements:
    (having_id |
    having_uuid | 
    having_oid |
    having_status)
    ;

fact_having_statements: 
    (having_id |
    having_type |
    having_negation)
    (having_priority)?;

logic_having_statements:
    (having_statements | 
    having_context );

protocol_having_statements:
    having_id
    having_uuid
    having_oid
    (having_priority)?
    (having_scope)*;
    
having_id: (HAVING)?ID NAMED_ID;
having_uuid: (HAVING)?UUID (UUIDV4|STRING);
having_oid: (HAVING)?OID OID_DOTTED;
having_status: (HAVING)?STATUS STATUS_VAL;
having_type: (HAVING)?TYPE PRIMITIVE_TYPE;
having_negation: (HAVING)?NEGATION BOOL_VAL;
having_context: (HAVING)?CONTEXT CLASS_TYPE (when_guard_condition)?;
having_model: (HAVING)?MODEL (
    STRING |
    AS
    (HAVING FORMAT FORMAT_REF)?
    MULTILINE_STRING
    END (MODEL)?
);
having_priority: (HAVING)?PRIORITY (INTEGER)?;
having_scope: (HAVING)?SCOPE (STRING|NAMED_ID);

library_definitions: 
    logic_block | 
    data_block;
    
logic_block: (DEFINE)?LOGIC STRING
    (logic_having_statements)*
    (metadata_statement)?
    AS
    (logic_definitions)+
    END (LOGIC)?;

data_block: (DEFINE)?DATA STRING
    (having_statements)*
    (metadata_statement)?
    AS
    MULTILINE_STRING
    END (DATA)?;
 
logic_definitions:
    define_fact |
    define_model |
    define_rule | 
    define_protocol;

define_fact: (DEFINE)?FACT STRING
    (fact_having_statements)*
    (metadata_statement)?
    AS
    fact_computation
    (fact_normalization)*
    END (FACT)?;

fact_normalization: NORMALIZE (when_guard_condition)? COMPUTEDBY
    csharp_logic;

fact_computation:
    csharp_logic |
    hdsi_logic |
    query_logic |
    all_logic |
    none_logic |
    any_logic;

fact_reference_or_computation: 
    fact_computation |
    NAMED_ID |
    STRING;

csharp_logic: 'csharp(' (STRING|MULTILINE_STRING) ')';
hdsi_logic: 'hdsi(' (STRING|MULTILINE_STRING) ('scoped-to' (CONTEXT|PROPOSAL))? (NEGATED)? ')';
query_logic: 'query('
    'from' hdsi_logic 
    'where' hdsi_logic 
    'select' (AGG_SELECTOR)? hdsi_logic
    ('order by' hdsi_logic)? 
    ')';
all_logic: 'all('
    (fact_reference_or_computation ','?)*
    ')';
any_logic: 'any('
    (fact_reference_or_computation ','?)*
    ')';
none_logic: 'none('
    (fact_reference_or_computation ','?)*
    ')';

define_model: (DEFINE)?MODEL STRING
    (having_statements)*
    (HAVING FORMAT FORMAT_REF)?
    (metadata_statement)?
    AS
    MULTILINE_STRING
    END (MODEL)?;

define_rule: (DEFINE)?RULE STRING
    (having_statements)*
    (having_priority)?
    (metadata_statement)?
    AS
    (when_guard_condition)?
    THEN (then_action_statements)*
    END (RULE)?;

then_action_statements: (
    propose_action_statement |
    raise_action_statement |
    assign_action_statement | 
    apply_action_statement | 
    repeat_action_statement
    );

repeat_action_statement:
    REPEAT 
    (UNTIL fact_reference_or_computation |
    FOR INTEGER (ITERATIONS)? (TRACKBY LITERAL)?)
        (then_action_statements)*
    END (REPEAT)?;

propose_action_statement: 
    PROPOSE (STRING)?
        (having_model)
        (metadata_statement)?
    AS
        (assign_action_statement)*
    END (PROPOSE)?;

raise_action_statement:
    RAISE (STRING|MULTILINE_STRING)
        (HAVING PRIORITY ISSUE_PRIORITY_VAL)?
        (HAVING TYPE (STRING|UUIDV4))?
        (metadata_statement)?
    ;

assign_action_statement:
    ASSIGN (csharp_logic|hdsi_logic|STRING|CONST (INTEGER|FLOAT|STRING|UUIDV4|BOOL_VAL)) TO LITERAL;

apply_action_statement: APPLY STRING;

define_protocol: 
    (DEFINE)? PROTOCOL STRING
    protocol_having_statements
    (metadata_statement)?
    AS
        (when_guard_condition)?
        THEN (then_action_statements|inline_rule_definition)*
    END (PROTOCOL)?;

inline_rule_definition: RULE (metadata_statement)? AS
    (when_guard_condition)?
    THEN (then_action_statements)*
    END RULE;

metadata_statement: (WITH)?METADATA
    (metadata_author_statement|
    metadata_version_statement|
    metadata_documentation_statement)*
    END (METADATA)?;

metadata_author_statement:
    AUTHOR;

metadata_documentation_statement:
    DOCUMENTATION;

metadata_version_statement:
    VERSION (VERSION_VAL|FLOAT);

when_guard_condition:
    WHEN fact_reference_or_computation;

REPEAT: 'repeat';
UNTIL: 'until';
TRACKBY: 'track-by';
NORMALIZE: 'normalize';
COMPUTEDBY: 'computed';
FOR: 'for';
METADATA: 'metadata';
DOCUMENTATION: 'doc' (~[\r\n\u2028\u2029])*;
AUTHOR: 'author' (~[\r\n\u2028\u2029])*;
VERSION: 'version';
TRUE: 'true';
FALSE: 'false';
ITERATIONS: 'iterations';
INCLUDE: 'include';
LIBRARY: 'library';
NEGATED: 'negated';
LOGIC: 'logic';
FACT: 'fact';
RULE: 'rule';
PROTOCOL: 'protocol';
PROPOSE: 'propose';
APPLY: 'apply';
SCOPE: 'scope';
PRIORITY: 'priority';
ASSIGN: 'assign';
TO: 'to';
CONST: 'const';
MODEL: 'model';
DATA: 'data';
END: 'end';
THEN: 'then';
RAISE: 'raise';
DEFINE: 'define';
PROPOSAL: 'proposal';
TYPE: 'type';
OID: 'oid';
ID: 'id';
FORMAT: 'format';
UUID: 'uuid';
STATUS: 'status';
CONTEXT: 'context';
NEGATION: 'negation';
HAVING: 'having';
WHEN: 'when';
WITH: 'with';
AS: 'as';
AGG_SELECTOR: 'first'|'last'|'single';
MULTILINE_STRING: '$$' .*? '$$';
OID_DOTTED: '"' ([0-9]+ '.'?)+? '"';
HEX_4: [0-9a-f][0-9a-f][0-9a-f][0-9a-f];

BOOL_VAL: ('true'|'false');
STATUS_VAL: ('active'|'trial-use'|'dont-use'|'retired');

PRIMITIVE_TYPE: (BOOL_TYPE|STRING_TYPE|DATE_TYPE|LONG_TYPE|REAL_TYPE|INTEGER_TYPE);
CLASS_TYPE: ('Patient'|'Person'|'Material'|'Place'|'ManufacturedMaterial'|'Entity'|'Act'|'QuantityObservation'|'TextObservation'|'CodedObservation'|'Procedure'|'SubstanceAdministration');
TYPE_REF: (PRIMITIVE_TYPE|CLASS_TYPE);
FORMAT_REF: ('json'|'xml');
BOOL_TYPE: 'bool';
STRING_TYPE: 'string';
DATE_TYPE: 'date';
LONG_TYPE: 'long';
REAL_TYPE: 'real';
INTEGER_TYPE: 'int';
ISSUE_PRIORITY_VAL: ('error'|'warn'|'info');

UUIDV4:
    '{' HEX_4 HEX_4 '-' HEX_4 '-' HEX_4 '-' HEX_4 '-' HEX_4 HEX_4 HEX_4 '}'
    ;

LITERAL
    : [a-z]([a-z.0-9_] | '[' | ']' )*
    ;

NAMED_ID
    : '<'LITERAL'>'
    ;

INTEGER
    : [0-9]+
    ;

FLOAT
    : [0-9]+'.'[0-9]*
    | '.'[0-9]+
    ;

VERSION_VAL
    : [0-9]+'.'[0-9]+'.'[0-9]+'.'[0-9]('-'LITERAL*)?
    | [0-9]+'.'[0-9]+'.'[0-9]('-'LITERAL)?
    | [0-9]+'.'[0-9]+('-'LITERAL)?
    ;
STRING
    : '"' (~["\r\n] | '""')* '"'
    ;

COMMENT
    : '//' (~[\r\n\u2028\u2029])* -> channel(HIDDEN);

SPACE
    : ([\t\r\n]|' ')+ -> channel(HIDDEN);
    