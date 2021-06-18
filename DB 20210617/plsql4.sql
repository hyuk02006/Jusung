set serveroutput on
accept num prompt '숫자를 입력 ->'
set verify off

declare
   i number(4) := 1;
 
begin
   for i in 1..9 loop
      dbms_output.put_line('&num* '|| i || '=' ||&num*i);
   end loop;
end;
/