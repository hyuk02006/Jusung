set serveroutput on
--*테이블에서 행을 가져오기(단일행) : 컬럼명은 변수로 사용할 수 없다
--select 칼럼명,... inti 변수,...from 테이블명,...;
accept num prompt '입력 ->'
set verify off

declare
	name sawon.saname%type;
begin
	select saname into name from sawon where sabun =&num;
	dbms_output.put_line('이름:'|| name);
end;
/

