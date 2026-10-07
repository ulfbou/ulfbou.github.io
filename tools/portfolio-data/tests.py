#!/usr/bin/env python3
import copy, importlib.util, json, tempfile, unittest
from pathlib import Path
ROOT=Path(__file__).parent
SPEC=importlib.util.spec_from_file_location('portfolio_data_cli',ROOT/'cli.py'); CLI=importlib.util.module_from_spec(SPEC); SPEC.loader.exec_module(CLI)
FIX=ROOT/'fixtures/valid/one-approved-claim'
def data(name): return json.loads((FIX/name).read_text())
class PrototypeTests(unittest.TestCase):
    def setUp(self): self.g=data('governance.json'); self.a=data('approved.json'); self.e=data('evidence.json')
    def code(self,g=None,a=None,e=None):
        with self.assertRaises(CLI.Violation) as caught: CLI.validate(g or self.g,a or self.a,e or self.e)
        return caught.exception.code
    def test_valid_fixture_is_deterministic(self):
        left=CLI.compile_index(self.g,self.a,self.e); right=CLI.compile_index(self.g,self.a,self.e)
        self.assertEqual(CLI.canonical(left),CLI.canonical(right))
    def test_inference_is_machine_disabled(self):
        g=copy.deepcopy(self.g); g['inference']['admittedAdapters']=['provider']; self.assertEqual('GOV-015',self.code(g=g))
    def test_reference_visibility_is_required(self):
        a=copy.deepcopy(self.a); a['projects'][0]['claims'][0]['evidenceRefs'][0]['visibility']='Private'; self.assertEqual('GOV-008',self.code(a=a))
    def test_observation_visibility_is_required(self):
        e=copy.deepcopy(self.e); e['observations'][0]['visibility']='Private'; e['packageId']='evp_'+CLI.digest(CLI.package_identity(e)); self.a['projects'][0]['claims'][0]['evidenceRefs'][0]['packageId']=e['packageId']; self.assertEqual('GOV-008',self.code(e=e))
    def test_scope_changes_package_identity(self):
        e=copy.deepcopy(self.e); e['scope']['includes'].append('docs/**'); self.assertEqual('GOV-017',self.code(e=e))
    def test_path_traversal_fails(self):
        e=copy.deepcopy(self.e); e['observations'][0]['path']='../secret'; e['packageId']='evp_'+CLI.digest(CLI.package_identity(e)); self.a['projects'][0]['claims'][0]['evidenceRefs'][0]['packageId']=e['packageId']; self.assertEqual('GOV-006',self.code(e=e))
    def test_verify_rejects_extra_claim(self):
        projection=CLI.compile_index(self.g,self.a,self.e); projection['projects']['dx-domain']['claims'].append({'id':'extra'})
        self.assertNotEqual(CLI.canonical(projection),CLI.canonical(CLI.compile_index(self.g,self.a,self.e)))
if __name__=='__main__': unittest.main()
