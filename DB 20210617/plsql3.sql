set serveroutput on
set verify off
declare
  i number:=1;
  j number:=1;

--accept 변수 prompt'문자열'		//값이 입력되면 변수에 담아서 프로그램 내에서 아용 : [&변수]형태로 사용

--accept num prompt '숫자를 입력 ->'
--begin
--dbms_output.put_line('입력된 수: '|| &num);
--end;		

--홀수 구구단 출력
begin
 for i in 1..9 loop
  begin
   for j in 1..9 loop
    dbms_output.put_line( i || ' x ' || j || ' = ' || i*j);
   end loop;
  end; 
 end loop;
end;
/

