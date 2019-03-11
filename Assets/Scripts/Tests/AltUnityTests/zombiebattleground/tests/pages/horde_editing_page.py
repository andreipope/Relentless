from base import CZBTests
import time
class Horde_Editing_Page(CZBTests):

    def __init__(self,altdriver,driver):
        self.altdriver=altdriver
        self.driver=driver
        self.horde_editing_page=self.get_horde_editing_page()
        self.back_button=self.get_back_button()
        self.save_button=self.get_save_button()
        self.army_button=self.get_army_button()
        self.buy_button=self.get_buy_button()
        self.army_panel=self.get_army_panel()
        self.horde_panel=self.get_horde_panel()
        self.deck_name_input_field=self.get_deck_name_input_field()
        self.card_amount_text=self.get_card_amout_text()
        self.army_left_arrow_button=self.get_army_left_arrow()
        self.army_right_arrow_button=self.get_army_right_arrow()
        self.horde_left_arrow_button=self.get_horde_left_arrow()
        self.horde_right_arrow_button=self.get_horde_right_arrow()
        self.horde_scroll_area=self.get_horde_scroll_area()
    
    def get_horde_editing_page(self):
        return self.altdriver.wait_for_element('HordeEditingPage(Clone)')
    def get_back_button(self):
        return self.altdriver.wait_for_element(self.horde_editing_page.name+'/Button_Back')
    def get_save_button(self):
        return self.altdriver.wait_for_element(self.horde_editing_page.name+'/Button_Save')
    def get_army_button(self):
        return self.altdriver.wait_for_element(self.horde_editing_page.name+'/Button_Army')
    def get_buy_button(self):
        return self.altdriver.wait_for_element(self.horde_editing_page.name+'/Button_Buy')
    def get_army_panel(self):
        return self.altdriver.wait_for_element(self.horde_editing_page.name+'/Army')
    def get_horde_panel(self):
        return self.altdriver.wait_for_element(self.horde_editing_page.name+'/Horde')
    def get_army_left_arrow(self):
        return self.altdriver.wait_for_element(self.army_panel.name+'/ArrowLeftButton')
    def get_army_right_arrow(self):
        return self.altdriver.wait_for_element(self.army_panel.name+'/ArrowRightButton')
    def get_horde_left_arrow(self):
        return self.altdriver.wait_for_element(self.horde_panel.name+'/ArrowLeftButton')
    def get_horde_right_arrow(self):
        return self.altdriver.wait_for_element(self.horde_panel.name+'/ArrowRightButton')
    
    def get_deck_name_input_field(self):
        return self.altdriver.wait_for_element(self.horde_editing_page.name+'/DeckTitleInputText')
    def get_card_amout_text(self):
        return self.altdriver.wait_for_element(self.horde_editing_page.name+'/CardsAmount/CardsAmountText')
    def get_horde_scroll_area(self):
        return self.altdriver.wait_for_element(self.horde_panel.name+'/ScrollArea')


    def press_army_left_arrow(self):
        self.button_pressed(self.army_left_arrow_button)
    def press_army_right_arrow(self):
        self.button_pressed(self.army_right_arrow_button)
    def press_horde_left_arrow(self):
        self.button_pressed(self.horde_left_arrow_button)
    def press_horde_right_arrow(self):
        self.button_pressed(self.horde_right_arrow_button)
    def press_save(self):
        self.button_pressed(self.save_button)
    def press_back(self):
        self.button_pressed(self.back_button)
    
    def press_army_button(self):
        self.button_pressed(self.press_army_button)
    def press_buy_button(self):
        self.button_pressed(self.buy_button)

    def get_cards_shown_in_army_panel(self):
        cards=[]
        print(self.army_panel.name+'/Cards/CreatureCard(Clone)')
        cards = self.altdriver.find_elements(self.army_panel.name+'/Cards/CreatureCard(Clone)')
        cards.extend(self.altdriver.find_elements(self.army_panel.name+'/Cards/ItemCard(Clone)'))
        return cards
    def get_cards_shown_in_horde_panel(self):
        cards = self.altdriver.find_elements(self.horde_panel.name+'/Cards/CreatureCard(Clone)')
        cards.extend(self.altdriver.find_elements(self.horde_panel.name+'/Cards/ItemCard(Clone)'))
        return cards
    def get_card_name(self,card_id):
        card_name=self.altdriver.find_element('id('+card_id+')/TitleText')
        return self.read_tmp_GUI_text(card_name)
    def get_card_from_army_panel(self,card_name):
        started_card_name=''
        cards=self.get_cards_shown_in_army_panel()
        for card in cards:
            cardName=self.get_card_name(card.id)
            if cardName==card_name:
                return card
            started_card_name=cardName
        while True:
            self.press_army_right_arrow()
            cards=self.get_cards_shown_in_army_panel()
            for card in cards:
                cardName=self.get_card_name(card.id)
                if cardName==card_name:
                    return card
                if started_card_name==card_name:
                    return None
    def can_card_be_added_to_horde(self,card):
        card_text_availability=self.altdriver.find_element('id('+card.id+')/AmountForArmy/Text')
        if not self.read_tmp_text(card_text_availability)=='0':
            return True
        return False

    def get_card_that_can_be_added_to_horde(self):
        while True:
            cards=[]
            print(str(len(cards))+"d22asdsadas")
            cards=self.get_cards_shown_in_army_panel()
            print(str(len(cards))+"dasdsadas")
            for card in cards:
                if self.can_card_be_added_to_horde(card):
                    return card
            self.press_army_right_arrow()
            time.sleep(1)
    def add_cards_to_horde(self,number_of_cards):
        for i in range(number_of_cards):
            card=self.get_card_that_can_be_added_to_horde()
            self.driver.swipe(card.x,card.mobileY,self.horde_scroll_area.x,self.horde_scroll_area.mobileY,2000)


        



    
    


