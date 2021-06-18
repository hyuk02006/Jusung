
--입력받은 사원번호의 사원 커미션을 변경하는 프로그램(테이블에 comm n(10) 컬럼 추가 후 작업)
--단, 커미션은 급여가 1000미만	-> 급여의 10%
	       	--   1000 ~ 2000 	-> 급여의 15%
		  --  2000초과	-> 급여의 20%
		--	null	-> 0
set serveroutput on
accept num prompt '입력->'
set verify off

declare 
	v_sapay sawon.sapay%type;
	v_comm sawon.comm%type;
begin
	select sapay into v_sapay from sawon where sabun = &num;

	if (v_sapay < 1000) then
		--update sawon set comm = v_sapay*0.1 where sabun = &num;
		v_comm:=v_sapay*0.1;
	elsif ( v_sapay <= 2000)  then
		--update sawon set comm = v_sapay*0.15 where sabun = &num;
		v_comm:=v_sapay*0.15;
	elsif(v_sapay > 2000) then
		--update sawon set comm = v_sapay*0.2 where sabun = &num;	
		v_comm:=v_sapay*0.2;
	else
		--update sawon set comm = null where sabun = &num;	
		v_comm:=0;
	end if;

	update sawon set comm =v_comm where sabun =&num;
	
end;
/